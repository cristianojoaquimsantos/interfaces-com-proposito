using FluentAssertions;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Tests;

public sealed class ItemPedidoTests
{
    [Fact]
    public void Deve_Calcular_Subtotal_Corretamente()
    {
        // Act
        var item = new ItemPedido(Guid.NewGuid(), "Headset", 250m, 3);

        // Assert
        item.Subtotal.Should().Be(750m);
        item.Quantidade.Should().Be(3);
        item.PrecoUnitario.Should().Be(250m);
    }

    [Fact]
    public void Nao_Deve_Permitir_Preco_Negativo()
    {
        // Act
        var act = () => new ItemPedido(Guid.NewGuid(), "Webcam", -10m, 1);

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*não pode ser negativo*");
    }

    [Fact]
    public void Nao_Deve_Permitir_Quantidade_Menor_Ou_Igual_A_Zero()
    {
        // Act
        var act = () => new ItemPedido(Guid.NewGuid(), "Mousepad", 40m, 0);

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*maior que zero*");
    }
}
