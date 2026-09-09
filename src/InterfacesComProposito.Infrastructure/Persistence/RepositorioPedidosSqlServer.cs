using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InterfacesComProposito.Infrastructure.Persistence;

/// <summary>
/// Implementação em SQL Server / EF Core para a porta IRepositorioPedidos.
/// 
/// NOTA ARQUITETURAL:
/// Carrega o Aggregate Root Pedido atomicamente com seus Itens.
/// A persistência respeita o limite do Aggregate.
/// </summary>
public sealed class RepositorioPedidosSqlServer : IRepositorioPedidos
{
    private readonly AppDbContext _context;

    public RepositorioPedidosSqlServer(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Pedido?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        await _context.Pedidos.AddAsync(pedido, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SalvarAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        if (_context.Entry(pedido).State == EntityState.Detached)
        {
            _context.Pedidos.Attach(pedido);
        }

        foreach (var item in pedido.Itens)
        {
            var itemEntry = _context.Entry(item);
            if (itemEntry.State == EntityState.Detached)
            {
                _context.ItensPedido.Add(item);
            }
            else if (itemEntry.State == EntityState.Modified && !_context.ItensPedido.Any(i => i.Id == item.Id))
            {
                itemEntry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
