namespace InterfacesComProposito.Application.Pedidos.CriarPedido;

public sealed record ItemPedidoCommand(
    Guid ProdutoId,
    int Quantidade);

public sealed record CriarPedidoCommand(
    Guid UsuarioId,
    IReadOnlyList<ItemPedidoCommand> Itens);

public sealed record CriarPedidoResponse(
    Guid PedidoId,
    decimal ValorTotal,
    string Status);
