using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InterfacesComProposito.Infrastructure.Persistence;

/// <summary>
/// Implementação em SQL Server / EF Core para a porta IRepositorioProdutos.
/// </summary>
public sealed class RepositorioProdutosSqlServer : IRepositorioProdutos
{
    private readonly AppDbContext _context;

    public RepositorioProdutosSqlServer(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Produto?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await _context.Produtos
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(produto);

        await _context.Produtos.AddAsync(produto, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
