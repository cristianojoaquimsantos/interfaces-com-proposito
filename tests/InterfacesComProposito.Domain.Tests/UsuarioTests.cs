using FluentAssertions;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Tests;

public sealed class UsuarioTests
{
    [Fact]
    public void Deve_Criar_Usuario_Ativo_Com_Sucesso()
    {
        // Act
        var usuario = new Usuario("Maria Silva", "maria.silva@exemplo.com");

        // Assert
        usuario.Nome.Should().Be("Maria Silva");
        usuario.Email.Should().Be("maria.silva@exemplo.com");
        usuario.Ativo.Should().BeTrue();
        usuario.CriadoEm.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ab")]
    public void Nao_Deve_Permitir_Nome_Invalido(string nomeInvalido)
    {
        // Act
        var act = () => new Usuario(nomeInvalido, "email@valido.com");

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*ao menos 3 caracteres*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("sem-ponto@com")]
    public void Nao_Deve_Permitir_Email_Invalido(string emailInvalido)
    {
        // Act
        var act = () => new Usuario("Nome Valido", emailInvalido);

        // Assert
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*inválido*");
    }

    [Fact]
    public void Deve_Inativar_Usuario_E_Impedir_Operacoes_Quando_Inativo()
    {
        // Arrange
        var usuario = new Usuario("Carlos Alberto", "carlos@exemplo.com");

        // Act
        usuario.Inativar();

        // Assert
        usuario.Ativo.Should().BeFalse();

        var act = () => usuario.GarantirAtivo();
        act.Should().Throw<RegraDeNegocioException>()
            .WithMessage("*inativo*");
    }
}
