using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Entities;

public sealed class ItemPedido
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string NomeProduto { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;

    public ItemPedido(Guid pedidoId, Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
    {
        ValidarQuantidade(quantidade);
        ValidarPreco(precoUnitario);

        if (pedidoId == Guid.Empty)
            throw new RegraDeNegocioException("Identificador de pedido inválido.");

        if (produtoId == Guid.Empty)
            throw new RegraDeNegocioException("Identificador de produto inválido.");

        if (string.IsNullOrWhiteSpace(nomeProduto))
            throw new RegraDeNegocioException("Nome do produto é obrigatório no item.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        ProdutoId = produtoId;
        NomeProduto = nomeProduto.Trim();
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
    }

    public ItemPedido(Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
        : this(Guid.NewGuid(), produtoId, nomeProduto, precoUnitario, quantidade)
    {
    }

    private ItemPedido()
    {
        NomeProduto = string.Empty;
    }

    internal void IncrementarQuantidade(int quantidade)
    {
        ValidarQuantidade(quantidade);
        Quantidade += quantidade;
    }

    internal void DefinirQuantidade(int quantidade)
    {
        ValidarQuantidade(quantidade);
        Quantidade = quantidade;
    }

    private static void ValidarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException("A quantidade de um item do pedido deve ser maior que zero.");
    }

    private static void ValidarPreco(decimal preco)
    {
        if (preco < 0)
            throw new RegraDeNegocioException("O preço unitário não pode ser negativo.");
    }
}
