using FluentAssertions;
using InterfacesComProposito.Application.Pedidos.CriarPedido;
using InterfacesComProposito.Application.Pricing;
using InterfacesComProposito.Application.Tests.Fakes;
using InterfacesComProposito.Application.Usuarios;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Application.Tests;

public sealed class CriarPedidoHandlerTests
{
    private readonly FakeRepositorioUsuarios _fakeUsuarios;
    private readonly FakeRepositorioPedidos _fakePedidos;
    private readonly FakeRepositorioProdutos _fakeProdutos;
    private readonly UsuarioService _usuarioService;
    private readonly CalculadoraDePrecoPadrao _calculadoraPreco;
    private readonly CriarPedidoHandler _handler;

    public CriarPedidoHandlerTests()
    {
        _fakeUsuarios = new FakeRepositorioUsuarios();
        _fakePedidos = new FakeRepositorioPedidos();
        _fakeProdutos = new FakeRepositorioProdutos();

        _usuarioService = new UsuarioService(_fakeUsuarios);
        _calculadoraPreco = new CalculadoraDePrecoPadrao();

        _handler = new CriarPedidoHandler(
            _usuarioService,
            _fakeProdutos,
            _fakePedidos,
            _calculadoraPreco);
    }

    [Fact]
    public async Task Deve_Criar_Pedido_Com_Sucesso_E_Calcular_Totais()
    {
        // Arrange
        var usuario = new Usuario("Lucas Mendes", "lucas@exemplo.com");
        await _fakeUsuarios.AdicionarAsync(usuario, CancellationToken.None);

        var produto1 = new Produto("Teclado Mecânico", 200m);
        var produto2 = new Produto("Mouse Gamer", 100m);
        _fakeProdutos.AdicionarParaTeste(produto1);
        _fakeProdutos.AdicionarParaTeste(produto2);

        var command = new CriarPedidoCommand(
            usuario.Id,
            new List<ItemPedidoCommand>
            {
                new(produto1.Id, 2), // 400
                new(produto2.Id, 1)  // 100
            });

        // Act
        var response = await _handler.ExecutarAsync(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ValorTotal.Should().Be(500m);

        var pedidoSalvo = await _fakePedidos.ObterAsync(response.PedidoId, CancellationToken.None);
        pedidoSalvo.Should().NotBeNull();
        pedidoSalvo!.Itens.Should().HaveCount(2);
        pedidoSalvo.ValorTotal.Should().Be(500m);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Se_Produto_Nao_Existir()
    {
        // Arrange
        var usuario = new Usuario("Lucas Mendes", "lucas@exemplo.com");
        await _fakeUsuarios.AdicionarAsync(usuario, CancellationToken.None);

        var command = new CriarPedidoCommand(
            usuario.Id,
            new List<ItemPedidoCommand>
            {
                new(Guid.NewGuid(), 1)
            });

        // Act
        var act = async () => await _handler.ExecutarAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RecursoNaoEncontradoException>();
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Se_Usuario_Estiver_Inativo()
    {
        // Arrange
        var usuario = new Usuario("Lucas Inativo", "inativo@exemplo.com");
        usuario.Inativar();
        await _fakeUsuarios.AdicionarAsync(usuario, CancellationToken.None);

        var produto = new Produto("Teclado", 150m);
        _fakeProdutos.AdicionarParaTeste(produto);

        var command = new CriarPedidoCommand(
            usuario.Id,
            new List<ItemPedidoCommand>
            {
                new(produto.Id, 1)
            });

        // Act
        var act = async () => await _handler.ExecutarAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RegraDeNegocioException>()
            .WithMessage("*inativo*");
    }
}
