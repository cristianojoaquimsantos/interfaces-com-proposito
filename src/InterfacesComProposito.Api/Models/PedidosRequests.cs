namespace InterfacesComProposito.Api.Models;

public sealed record ItemPedidoRequest(
    Guid ProdutoId,
    int Quantidade);

public sealed record CriarPedidoRequest(
    Guid UsuarioId,
    IReadOnlyList<ItemPedidoRequest> Itens);

public sealed record AdicionarItemPedidoRequest(
    Guid ProdutoId,
    int Quantidade);
