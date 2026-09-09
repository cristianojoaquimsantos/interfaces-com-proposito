using FluentAssertions;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Enums;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Tests;

public sealed class PedidoTests
{
    [Fact]
    public void Deve_Adicionar_Item_E_Recalcular_Total_Com_Sucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var pedido = new Pedido(usuarioId);
        var produtoId = Guid.NewGuid();

        // Act
        pedido.AdicionarItem(produtoId, "Teclado", 100m, 2);

        // Assert
        pedido.Itens.Should().HaveCount(1);
        pedido.ValorTotal.Should().Be(200m);
        pedido.Status.Should().Be(StatusPedido.Criado);
    }

    [Fact]
    public void Deve_Incrementar_Quantidade_Ao_Adicionar_Mesmo_Produto()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        var produtoId = Guid.NewGuid();

        // Act
        pedido.AdicionarItem(produtoId, "Mouse", 50m, 1);
        pedido.AdicionarItem(produtoId, "Mouse", 50m, 3);

        // Assert
        pedido.Itens.Should().HaveCount(1);
        pedido.Itens.First().Quantidade.Should().Be(4);
        pedido.ValorTotal.Should().Be(200m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Nao_Deve_Permitir_Adicionar_Item_Com_Quantidade_Invalida(int quantidadeInvalida)
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());

        // Act
        var act = () => pedido.AdicionarItem(Guid.NewGuid(), "Produto", 10m, quantidadeInvalida);

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void Nao_Deve_Finalizar_Pedido_Vazio()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());

        // Act
        var act = () => pedido.Finalizar();

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*sem itens*");
    }

    [Fact]
    public void Deve_Finalizar_Pedido_Com_Sucesso_E_Alterar_Status()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        pedido.AdicionarItem(Guid.NewGuid(), "Monitor", 1200m, 1);

        // Act
        pedido.Finalizar();

        // Assert
        pedido.Status.Should().Be(StatusPedido.Finalizado);
        pedido.FinalizadoEm.Should().NotBeNull();
        pedido.ValorTotal.Should().Be(1200m);
    }

    [Fact]
    public void Nao_Deve_Permitir_Modificar_Pedido_Finalizado()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        pedido.AdicionarItem(Guid.NewGuid(), "Cabo HDMI", 30m, 1);
        pedido.Finalizar();

        // Act
        var act = () => pedido.AdicionarItem(Guid.NewGuid(), "Adaptador", 20m, 1);

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*já finalizado*");
    }

    [Fact]
    public void Nao_Deve_Permitir_Cancelar_Pedido_Finalizado()
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid());
        pedido.AdicionarItem(Guid.NewGuid(), "Cabo", 30m, 1);
        pedido.Finalizar();

        // Act
        var act = () => pedido.Cancelar();

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*já foi finalizado*");
    }
}
