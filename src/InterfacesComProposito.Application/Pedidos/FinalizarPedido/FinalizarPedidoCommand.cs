namespace InterfacesComProposito.Application.Pedidos.FinalizarPedido;

public sealed record FinalizarPedidoCommand(Guid PedidoId);

public sealed record FinalizarPedidoResponse(
    Guid PedidoId,
    string Status,
    decimal ValorTotal,
    DateTimeOffset FinalizadoEm,
    string? TransacaoPagamentoId);
