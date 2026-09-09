using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using InterfacesComProposito.Api.Models;
using InterfacesComProposito.Application.Pedidos.AdicionarItemPedido;
using InterfacesComProposito.Application.Pedidos.AnexarComprovantePedido;
using InterfacesComProposito.Application.Pedidos.ConsultarPedido;
using InterfacesComProposito.Application.Pedidos.CriarPedido;
using InterfacesComProposito.Application.Pedidos.FinalizarPedido;
using InterfacesComProposito.Domain.Enums;
using InterfacesComProposito.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterfacesComProposito.Api.IntegrationTests;

public sealed class PedidosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PedidosControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompletoDePedido_Criacao_AdicaoDeItem_Finalizacao_E_UploadComprovante()
    {
        // -------------------------------------------------------------------------
        // 1. Criar Pedido (POST /api/pedidos)
        // -------------------------------------------------------------------------
        var criarRequest = new CriarPedidoRequest(
            DbInitializer.UsuarioExemploId,
            new List<ItemPedidoRequest>
            {
                new(DbInitializer.ProdutoTecladoId, 1) // 100.00
            });

        var criarResponse = await _client.PostAsJsonAsync("/api/pedidos", criarRequest);
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedidoCriado = await criarResponse.Content.ReadFromJsonAsync<CriarPedidoResponse>();
        pedidoCriado.Should().NotBeNull();
        pedidoCriado!.PedidoId.Should().NotBeEmpty();
        pedidoCriado.ValorTotal.Should().Be(350.00m);

        var pedidoId = pedidoCriado.PedidoId;

        // -------------------------------------------------------------------------
        // 2. Consultar Pedido (GET /api/pedidos/{id})
        // -------------------------------------------------------------------------
        var getResponse = await _client.GetAsync($"/api/pedidos/{pedidoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pedidoDto = await getResponse.Content.ReadFromJsonAsync<PedidoDto>();
        pedidoDto.Should().NotBeNull();
        pedidoDto!.Itens.Should().HaveCount(1);
        pedidoDto.Status.Should().Be(StatusPedido.Criado.ToString());

        // -------------------------------------------------------------------------
        // 3. Adicionar Item ao Pedido (POST /api/pedidos/{id}/itens)
        // -------------------------------------------------------------------------
        var adicionarItemRequest = new AdicionarItemPedidoRequest(
            DbInitializer.ProdutoMouseId, // 180.00
            2);

        var addItemResponse = await _client.PostAsJsonAsync($"/api/pedidos/{pedidoId}/itens", adicionarItemRequest);
        addItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var addItemResult = await addItemResponse.Content.ReadFromJsonAsync<AdicionarItemPedidoResponse>();
        addItemResult.Should().NotBeNull();
        addItemResult!.NovoValorTotal.Should().Be(710.00m); // 350 + (180 * 2) = 710.00

        // -------------------------------------------------------------------------
        // 4. Anexar Comprovante via IFileStorage (POST /api/pedidos/{id}/comprovante)
        // -------------------------------------------------------------------------
        using var multipartContent = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("Conteúdo simulado do comprovante bancário");
        var byteContent = new ByteArrayContent(fileBytes);
        byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        multipartContent.Add(byteContent, "arquivo", "comprovante_pagamento.pdf");

        var uploadResponse = await _client.PostAsync($"/api/pedidos/{pedidoId}/comprovante", multipartContent);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<AnexarComprovantePedidoResponse>();
        uploadResult.Should().NotBeNull();
        uploadResult!.ComprovanteUrl.Should().NotBeNullOrWhiteSpace();

        // -------------------------------------------------------------------------
        // 5. Finalizar Pedido (POST /api/pedidos/{id}/finalizar)
        // -------------------------------------------------------------------------
        var finalizarResponse = await _client.PostAsync($"/api/pedidos/{pedidoId}/finalizar", null);
        finalizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizarResult = await finalizarResponse.Content.ReadFromJsonAsync<FinalizarPedidoResponse>();
        finalizarResult.Should().NotBeNull();
        finalizarResult!.Status.Should().Be(StatusPedido.Finalizado.ToString());
        finalizarResult.TransacaoPagamentoId.Should().NotBeNullOrWhiteSpace();

        // -------------------------------------------------------------------------
        // 6. Tentar Finalizar Novamente deve gerar 409 Conflict (ProblemDetails)
        // -------------------------------------------------------------------------
        var refinalizarResponse = await _client.PostAsync($"/api/pedidos/{pedidoId}/finalizar", null);
        refinalizarResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var conflictProblem = await refinalizarResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        conflictProblem.Should().NotBeNull();
        conflictProblem!.Status.Should().Be(StatusCodes.Status409Conflict);
        conflictProblem.Title.Should().Be("Violação de regra de negócio");
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400_QuandoItensForemVazios()
    {
        // Arrange
        var request = new CriarPedidoRequest(DbInitializer.UsuarioExemploId, new List<ItemPedidoRequest>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
    }
}
