namespace InterfacesComProposito.Application.Pedidos.AdicionarItemPedido;

public sealed record AdicionarItemPedidoCommand(
    Guid PedidoId,
    Guid ProdutoId,
    int Quantidade);

public sealed record AdicionarItemPedidoResponse(
    Guid PedidoId,
    decimal NovoValorTotal,
    int QuantidadeItens);
