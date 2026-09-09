using FluentAssertions;
using InterfacesComProposito.Domain.Exceptions;
using InterfacesComProposito.Domain.ValueObjects;

namespace InterfacesComProposito.Domain.Tests;

public sealed class EnderecoTests
{
    [Fact]
    public void Deve_Criar_Endereco_Valido()
    {
        // Act
        var endereco = new Endereco(
            "Rua das Flores",
            "123",
            "Apto 1",
            "Centro",
            "Curitiba",
            "PR",
            "80000-000");

        // Assert
        endereco.Logradouro.Should().Be("Rua das Flores");
        endereco.Numero.Should().Be("123");
        endereco.Complemento.Should().Be("Apto 1");
        endereco.Bairro.Should().Be("Centro");
        endereco.Cidade.Should().Be("Curitiba");
        endereco.Estado.Should().Be("PR");
        endereco.Cep.Should().Be("80000-000");
    }

    [Theory]
    [InlineData("", "123", "Bairro", "Cidade", "SP", "01000-000")]
    [InlineData("Rua", "", "Bairro", "Cidade", "SP", "01000-000")]
    [InlineData("Rua", "123", "", "Cidade", "SP", "01000-000")]
    [InlineData("Rua", "123", "Bairro", "", "SP", "01000-000")]
    [InlineData("Rua", "123", "Bairro", "Cidade", "SPO", "01000-000")]
    [InlineData("Rua", "123", "Bairro", "Cidade", "SP", "")]
    public void Nao_Deve_Permitir_Endereco_Incompleto(
        string logradouro,
        string numero,
        string bairro,
        string cidade,
        string estado,
        string cep)
    {
        // Act
        var act = () => new Endereco(logradouro, numero, null, bairro, cidade, estado, cep);

        // Assert
        act.Should().Throw<RegraDeNegocioException>();
    }
}
