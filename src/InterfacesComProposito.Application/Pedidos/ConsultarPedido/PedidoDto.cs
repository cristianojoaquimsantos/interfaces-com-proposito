namespace InterfacesComProposito.Application.Pedidos.ConsultarPedido;

public sealed record ItemPedidoDto(
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoUnitario,
    int Quantidade,
    decimal Subtotal);

public sealed record PedidoDto(
    Guid Id,
    Guid UsuarioId,
    string Status,
    decimal ValorTotal,
    DateTimeOffset CriadoEm,
    DateTimeOffset? FinalizadoEm,
    string? ComprovanteUrl,
    IReadOnlyList<ItemPedidoDto> Itens);
