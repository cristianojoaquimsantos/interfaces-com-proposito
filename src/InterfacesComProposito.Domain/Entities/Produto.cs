using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Entities;

public sealed class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public decimal Preco { get; private set; }
    public bool Ativo { get; private set; }

    public Produto(string nome, decimal preco)
    {
        ValidarNome(nome);
        ValidarPreco(preco);

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Preco = preco;
        Ativo = true;
    }

    public Produto(Guid id, string nome, decimal preco, bool ativo = true)
    {
        ValidarNome(nome);
        ValidarPreco(preco);

        Id = id;
        Nome = nome.Trim();
        Preco = preco;
        Ativo = ativo;
    }

    private Produto()
    {
        Nome = string.Empty;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void AtualizarPreco(decimal novoPreco)
    {
        ValidarPreco(novoPreco);
        Preco = novoPreco;
    }

    public void AtualizarNome(string novoNome)
    {
        ValidarNome(novoNome);
        Nome = novoNome.Trim();
    }

    public void GarantirDisponivelParaVenda()
    {
        if (!Ativo)
            throw new RegraDeNegocioException($"O produto '{Nome}' está inativo e não pode ser adicionado ao pedido.");
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioException("Nome do produto é obrigatório.");
    }

    private static void ValidarPreco(decimal preco)
    {
        if (preco < 0)
            throw new RegraDeNegocioException("O preço do produto não pode ser negativo.");
    }
}
