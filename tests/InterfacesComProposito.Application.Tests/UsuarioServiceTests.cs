using FluentAssertions;
using InterfacesComProposito.Application.Tests.Fakes;
using InterfacesComProposito.Application.Usuarios;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Application.Tests;

public sealed class UsuarioServiceTests
{
    private readonly FakeRepositorioUsuarios _fakeRepositorio;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceTests()
    {
        _fakeRepositorio = new FakeRepositorioUsuarios();

        // Instanciação direta da classe concreta UsuarioService sem interface
        _usuarioService = new UsuarioService(_fakeRepositorio);
    }

    [Fact]
    public async Task Deve_Obter_Usuario_Valido_Quando_Ativo()
    {
        // Arrange
        var usuario = new Usuario("Roberto Carlos", "roberto@exemplo.com");
        await _fakeRepositorio.AdicionarAsync(usuario, CancellationToken.None);

        // Act
        var resultado = await _usuarioService.ObterUsuarioValidoAsync(usuario.Id, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(usuario.Id);
        resultado.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Existe()
    {
        // Act
        var act = async () => await _usuarioService.ObterUsuarioValidoAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RecursoNaoEncontradoException>();
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var usuario = new Usuario("Ana Paula", "ana@exemplo.com");
        usuario.Inativar();
        await _fakeRepositorio.AdicionarAsync(usuario, CancellationToken.None);

        // Act
        var act = async () => await _usuarioService.ObterUsuarioValidoAsync(usuario.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RegraDeNegocioException>()
            .WithMessage("*inativo*");
    }

    [Fact]
    public async Task Deve_Criar_Usuario_Com_Sucesso()
    {
        // Act
        var usuarioCriado = await _usuarioService.CriarUsuarioAsync(
            "Marcos Santos",
            "marcos@exemplo.com",
            null,
            CancellationToken.None);

        // Assert
        usuarioCriado.Should().NotBeNull();
        usuarioCriado.Nome.Should().Be("Marcos Santos");

        var noBanco = await _fakeRepositorio.ObterAsync(usuarioCriado.Id, CancellationToken.None);
        noBanco.Should().NotBeNull();
    }
}
