using InterfacesComProposito.Domain.Enums;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Entities;

public sealed class Pedido
{
    private readonly List<ItemPedido> _itens = [];

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();
    public StatusPedido Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset? FinalizadoEm { get; private set; }
    public string? ComprovanteUrl { get; private set; }

    public Pedido(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new RegraDeNegocioException("Identificador de usuário inválido.");

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Status = StatusPedido.Criado;
        ValorTotal = 0m;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    private Pedido()
    {
    }

    public void AdicionarItem(Guid produtoId, string nomeProduto, decimal precoUnitario, int quantidade)
    {
        GarantirPermiteModificacao();

        var itemExistente = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (itemExistente is not null)
        {
            itemExistente.IncrementarQuantidade(quantidade);
        }
        else
        {
            var novoItem = new ItemPedido(Id, produtoId, nomeProduto, precoUnitario, quantidade);
            _itens.Add(novoItem);
        }

        CalcularTotal();
    }

    public void RemoverItem(Guid produtoId)
    {
        GarantirPermiteModificacao();

        var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item is null)
            throw new RegraDeNegocioException($"Produto '{produtoId}' não encontrado no pedido.");

        _itens.Remove(item);
        CalcularTotal();
    }

    public void CalcularTotal()
    {
        ValorTotal = _itens.Sum(i => i.Subtotal);
    }

    public void Finalizar()
    {
        if (Status == StatusPedido.Finalizado)
            throw new RegraDeNegocioException("O pedido já foi finalizado anteriormente.");

        if (Status == StatusPedido.Cancelado)
            throw new RegraDeNegocioException("Não é possível finalizar um pedido que está cancelado.");

        if (_itens.Count == 0)
            throw new RegraDeNegocioException("Não é possível finalizar um pedido sem itens.");

        CalcularTotal();

        if (ValorTotal <= 0)
            throw new RegraDeNegocioException("O valor total do pedido deve ser maior que zero para finalização.");

        Status = StatusPedido.Finalizado;
        FinalizadoEm = DateTimeOffset.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusPedido.Finalizado)
            throw new RegraDeNegocioException("Não é permitido cancelar um pedido que já foi finalizado.");

        if (Status == StatusPedido.Cancelado)
            return;

        Status = StatusPedido.Cancelado;
    }

    public void AnexarComprovante(string comprovanteUrl)
    {
        if (string.IsNullOrWhiteSpace(comprovanteUrl))
            throw new RegraDeNegocioException("URL ou caminho do comprovante não pode ser vazio.");

        ComprovanteUrl = comprovanteUrl.Trim();
    }

    private void GarantirPermiteModificacao()
    {
        if (Status == StatusPedido.Finalizado)
            throw new RegraDeNegocioException("Não é permitido alterar itens de um pedido já finalizado.");

        if (Status == StatusPedido.Cancelado)
            throw new RegraDeNegocioException("Não é permitido alterar itens de um pedido cancelado.");
    }
}
