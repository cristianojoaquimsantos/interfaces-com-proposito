namespace InterfacesComProposito.Pricing.Contracts;

public sealed record PedidoParaCalculo(
    Guid PedidoId,
    Guid UsuarioId,
    IReadOnlyList<ItemParaCalculo> Itens);
