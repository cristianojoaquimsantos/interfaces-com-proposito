namespace InterfacesComProposito.Application.Ports.Security;

/// <summary>
/// Porta para recuperação segura de segredos e credenciais de integração.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Fronteira de infraestrutura e isolamento de segurança.
/// Permite que componentes e adapters recuperem credenciais dinâmicas em tempo de execução
/// sem expor segredos em arquivos de configuração estáticos e sem acoplar o sistema à API do Azure Key Vault.
/// </summary>
public interface ISecretProvider
{
    Task<string?> ObterAsync(
        string nome,
        CancellationToken cancellationToken);
}
