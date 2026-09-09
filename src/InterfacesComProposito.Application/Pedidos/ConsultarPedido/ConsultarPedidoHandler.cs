using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Application.Pedidos.ConsultarPedido;

/// <summary>
/// Handler concreto para consulta de pedidos.
/// 
/// NOTA ARQUITETURAL:
/// Depende exclusivamente da porta IRepositorioPedidos.
/// Não possui interface "IConsultarPedidoHandler" para evitar overengineering.
/// </summary>
public sealed class ConsultarPedidoHandler
{
    private readonly IRepositorioPedidos _repositorioPedidos;

    public ConsultarPedidoHandler(IRepositorioPedidos repositorioPedidos)
    {
        _repositorioPedidos = repositorioPedidos ?? throw new ArgumentNullException(nameof(repositorioPedidos));
    }

    public async Task<PedidoDto> ExecutarAsync(Guid pedidoId, CancellationToken cancellationToken)
    {
        var pedido = await _repositorioPedidos.ObterAsync(pedidoId, cancellationToken);
        if (pedido is null)
            throw RecursoNaoEncontradoException.Para<Pedido>(pedidoId);

        var itensDto = pedido.Itens.Select(i => new ItemPedidoDto(
            i.ProdutoId,
            i.NomeProduto,
            i.PrecoUnitario,
            i.Quantidade,
            i.Subtotal)).ToList();

        return new PedidoDto(
            pedido.Id,
            pedido.UsuarioId,
            pedido.Status.ToString(),
            pedido.ValorTotal,
            pedido.CriadoEm,
            pedido.FinalizadoEm,
            pedido.ComprovanteUrl,
            itensDto);
    }
}
