namespace InterfacesComProposito.Application.Pedidos.Eventos;

public sealed record PedidoFinalizadoEvent(
    Guid PedidoId,
    Guid UsuarioId,
    decimal ValorTotal,
    DateTimeOffset FinalizadoEm);
