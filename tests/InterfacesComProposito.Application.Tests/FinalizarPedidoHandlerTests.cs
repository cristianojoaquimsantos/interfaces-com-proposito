using FluentAssertions;
using InterfacesComProposito.Application.Pedidos.Eventos;
using InterfacesComProposito.Application.Pedidos.FinalizarPedido;
using InterfacesComProposito.Application.Ports.Audit;
using InterfacesComProposito.Application.Ports.Messaging;
using InterfacesComProposito.Application.Ports.Notifications;
using InterfacesComProposito.Application.Ports.Payments;
using InterfacesComProposito.Application.Tests.Fakes;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Enums;
using InterfacesComProposito.Domain.Exceptions;
using Moq;

namespace InterfacesComProposito.Application.Tests;

public sealed class FinalizarPedidoHandlerTests
{
    private readonly FakeRepositorioPedidos _fakePedidos;
    private readonly Mock<IPagamentoGateway> _mockPagamentoGateway;
    private readonly Mock<IMessagePublisher> _mockMessagePublisher;
    private readonly Mock<INotificador> _mockNotificador;
    private readonly Mock<IAuditoria> _mockAuditoria;
    private readonly FinalizarPedidoHandler _handler;

    public FinalizarPedidoHandlerTests()
    {
        _fakePedidos = new FakeRepositorioPedidos();
        _mockPagamentoGateway = new Mock<IPagamentoGateway>();
        _mockMessagePublisher = new Mock<IMessagePublisher>();
        _mockNotificador = new Mock<INotificador>();
        _mockAuditoria = new Mock<IAuditoria>();

        _handler = new FinalizarPedidoHandler(
            _fakePedidos,
            _mockPagamentoGateway.Object,
            _mockMessagePublisher.Object,
            _mockNotificador.Object,
            _mockAuditoria.Object);
    }

    [Fact]
    public async Task Deve_Finalizar_Pedido_Processar_Pagamento_E_Publicar_Evento_Com_Sucesso()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        pedido.AdicionarItem(Guid.NewGuid(), "Monitor 4K", 2500m, 1);
        await _fakePedidos.AdicionarAsync(pedido, CancellationToken.None);

        _mockPagamentoGateway
            .Setup(p => p.ProcessarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoPagamento(true, "TX_12345", "Aprovado"));

        var command = new FinalizarPedidoCommand(pedido.Id);

        // Act
        var response = await _handler.ExecutarAsync(command, CancellationToken.None);

        // Assert - Comportamento observável e estado final
        response.Should().NotBeNull();
        response.Status.Should().Be(StatusPedido.Finalizado.ToString());
        response.TransacaoPagamentoId.Should().Be("TX_12345");

        var pedidoFinal = await _fakePedidos.ObterAsync(pedido.Id, CancellationToken.None);
        pedidoFinal!.Status.Should().Be(StatusPedido.Finalizado);
        pedidoFinal.FinalizadoEm.Should().NotBeNull();

        // Mocks observam apenas fronteiras externas de I/O
        _mockMessagePublisher.Verify(m => m.PublicarAsync(
            It.Is<PedidoFinalizadoEvent>(e => e.PedidoId == pedido.Id && e.ValorTotal == 2500m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Se_Pagamento_For_Recusado()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        pedido.AdicionarItem(Guid.NewGuid(), "SSD 1TB", 600m, 1);
        await _fakePedidos.AdicionarAsync(pedido, CancellationToken.None);

        _mockPagamentoGateway
            .Setup(p => p.ProcessarAsync(It.IsAny<Pagamento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoPagamento(false, null, "Saldo insuficiente no cartão"));

        var command = new FinalizarPedidoCommand(pedido.Id);

        // Act
        var act = async () => await _handler.ExecutarAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RegraDeNegocioException>()
            .WithMessage("*Saldo insuficiente*");

        // O pedido não deve ser finalizado nem o evento disparado
        _mockMessagePublisher.Verify(m => m.PublicarAsync(It.IsAny<PedidoFinalizadoEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Se_Pedido_Estiver_Vazio()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        await _fakePedidos.AdicionarAsync(pedido, CancellationToken.None);

        var command = new FinalizarPedidoCommand(pedido.Id);

        // Act
        var act = async () => await _handler.ExecutarAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RegraDeNegocioException>()
            .WithMessage("*sem itens*");
    }
}
