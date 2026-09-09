using InterfacesComProposito.Domain.Entities;

namespace InterfacesComProposito.Application.Ports.Persistence;

/// <summary>
/// Porta de persistência para consulta e manutenção do catálogo de Produtos.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Inversão de dependência (DIP). Permite que os casos de uso consultem preços e produtos
/// ativos sem expor DbContext ou consultas diretas à tabela de produtos.
/// </summary>
public interface IRepositorioProdutos
{
    Task<Produto?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken);
}
