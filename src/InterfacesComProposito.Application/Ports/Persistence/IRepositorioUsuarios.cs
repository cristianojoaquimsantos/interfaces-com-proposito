using InterfacesComProposito.Domain.Entities;

namespace InterfacesComProposito.Application.Ports.Persistence;

/// <summary>
/// Porta de persistência para Usuários.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Esta interface existe para aplicar o Dependency Inversion Principle (DIP).
/// A camada Application define a abstração necessária para recuperar e persistir o estado do Usuário,
/// invertendo a dependência para que Application NÃO dependa do Entity Framework Core ou de SQL Server.
/// </summary>
public interface IRepositorioUsuarios
{
    Task<Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken);
    Task SalvarAsync(Usuario usuario, CancellationToken cancellationToken);
}
