namespace InterfacesComProposito.Application.Ports.Storage;

/// <summary>
/// Porta de armazenamento de arquivos.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Isolamento de tecnologia externa. A Application precisa persistir streams de arquivos (como comprovantes de pedidos)
/// e obter um identificador/URI sem se acoplar a SDKs de nuvem específicos (Azure Blob Storage, AWS S3, etc.).
/// Os casos de uso lidam com Streams e nomes lógicos, mantendo a regra de negócio agnóstica ao provedor.
/// </summary>
public interface IFileStorage
{
    Task<string> SalvarAsync(
        Stream arquivo,
        string nome,
        CancellationToken cancellationToken);
}
