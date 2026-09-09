using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InterfacesComProposito.Infrastructure.Persistence;

/// <summary>
/// Implementação em SQL Server / EF Core para a porta IRepositorioUsuarios.
/// 
/// NOTA ARQUITETURAL:
/// Esta classe é um Adapter da camada Infrastructure que atende ao contrato definido pela Application.
/// A inversão de dependência (DIP) garante que a Application desconheça a existência do EF Core e do SQL Server.
/// </summary>
public sealed class RepositorioUsuariosSqlServer : IRepositorioUsuarios
{
    private readonly AppDbContext _context;

    public RepositorioUsuariosSqlServer(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SalvarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (_context.Entry(usuario).State == EntityState.Detached)
        {
            _context.Usuarios.Update(usuario);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
