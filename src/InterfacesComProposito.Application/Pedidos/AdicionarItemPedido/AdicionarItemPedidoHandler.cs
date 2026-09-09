using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Application.Pedidos.AdicionarItemPedido;

/// <summary>
/// Handler concreto para adição de item a um pedido existente.
/// </summary>
public sealed class AdicionarItemPedidoHandler
{
    private readonly IRepositorioPedidos _repositorioPedidos;
    private readonly IRepositorioProdutos _repositorioProdutos;

    public AdicionarItemPedidoHandler(
        IRepositorioPedidos repositorioPedidos,
        IRepositorioProdutos repositorioProdutos)
    {
        _repositorioPedidos = repositorioPedidos ?? throw new ArgumentNullException(nameof(repositorioPedidos));
        _repositorioProdutos = repositorioProdutos ?? throw new ArgumentNullException(nameof(repositorioProdutos));
    }

    public async Task<AdicionarItemPedidoResponse> ExecutarAsync(AdicionarItemPedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await _repositorioPedidos.ObterAsync(command.PedidoId, cancellationToken);
        if (pedido is null)
            throw RecursoNaoEncontradoException.Para<Pedido>(command.PedidoId);

        var produto = await _repositorioProdutos.ObterAsync(command.ProdutoId, cancellationToken);
        if (produto is null)
            throw RecursoNaoEncontradoException.Para<Produto>(command.ProdutoId);

        produto.GarantirDisponivelParaVenda();

        // O domínio executa a regra e recalcula o total do agregado
        pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco, command.Quantidade);

        await _repositorioPedidos.SalvarAsync(pedido, cancellationToken);

        return new AdicionarItemPedidoResponse(
            pedido.Id,
            pedido.ValorTotal,
            pedido.Itens.Count);
    }
}
