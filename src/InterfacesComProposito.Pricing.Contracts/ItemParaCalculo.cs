namespace InterfacesComProposito.Pricing.Contracts;

public sealed record ItemParaCalculo(
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoUnitario,
    int Quantidade);
