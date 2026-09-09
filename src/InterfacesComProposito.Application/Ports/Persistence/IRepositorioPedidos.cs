using InterfacesComProposito.Domain.Entities;

namespace InterfacesComProposito.Application.Ports.Persistence;

/// <summary>
/// Porta de persistência para o Aggregate Root Pedido.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Inversão de dependência (DIP). O Aggregate Root Pedido e suas invariantes devem ser carregados
/// e persistidos atomicamente sem acoplamento entre os casos de uso e a tecnologia de banco de dados.
/// Não é um repositório genérico; atende exclusivamente às necessidades do ciclo de vida do Pedido.
/// </summary>
public interface IRepositorioPedidos
{
    Task<Pedido?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken);
    Task SalvarAsync(Pedido pedido, CancellationToken cancellationToken);
}
