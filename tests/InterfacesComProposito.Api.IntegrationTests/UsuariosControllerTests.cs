using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InterfacesComProposito.Application.Usuarios.ConsultarUsuario;
using InterfacesComProposito.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterfacesComProposito.Api.IntegrationTests;

public sealed class UsuariosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsuariosControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ObterUsuarioPorId_DeveRetornar200_QuandoUsuarioExiste()
    {
        // Act
        var response = await _client.GetAsync($"/api/usuarios/{DbInitializer.UsuarioExemploId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuario = await response.Content.ReadFromJsonAsync<ConsultarUsuarioDto>();
        usuario.Should().NotBeNull();
        usuario!.Id.Should().Be(DbInitializer.UsuarioExemploId);
        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task ObterUsuarioPorId_DeveRetornar404_QuandoUsuarioNaoExiste()
    {
        // Act
        var response = await _client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Recurso não encontrado");
    }
}
