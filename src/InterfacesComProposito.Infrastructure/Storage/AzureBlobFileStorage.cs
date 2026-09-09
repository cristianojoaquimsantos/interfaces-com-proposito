using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InterfacesComProposito.Application.Ports.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Storage;

/// <summary>
/// Implementação de IFileStorage utilizando Azure Blob Storage.
/// 
/// NOTA ARQUITETURAL:
/// Este adapter da camada Infrastructure encapsula completamente os SDKs oficiais da Microsoft
/// (BlobServiceClient, BlobContainerClient, etc.).
/// A camada Application apenas consome Streams e URLs através da porta IFileStorage.
/// Possui suporte a execução autônoma local quando não houver assinatura Azure configurada.
/// </summary>
public sealed class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobFileStorage> _logger;
    private readonly bool _useAzure;

    public AzureBlobFileStorage(IConfiguration configuration, ILogger<AzureBlobFileStorage> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var connectionString = configuration["Azure:BlobStorage:ConnectionString"];
        _containerName = configuration["Azure:BlobStorage:ContainerName"] ?? "comprovantes-pedidos";

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains("YOUR_"))
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _useAzure = true;
        }
        else
        {
            _useAzure = false;
            _logger.LogInformation("Azure Blob Storage não configurado com credenciais válidas. Utilizando armazenamento simulado para execução local.");
        }
    }

    public async Task<string> SalvarAsync(Stream arquivo, string nome, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arquivo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);

        if (_useAzure && _blobServiceClient is not null)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(nome);
            await blobClient.UploadAsync(arquivo, overwrite: true, cancellationToken);

            return blobClient.Uri.ToString();
        }

        // Simulação local autônoma para desenvolvimento sem Azure
        var localDir = Path.Combine(Path.GetTempPath(), "InterfacesComProposito", "Storage", _containerName);
        Directory.CreateDirectory(localDir);

        var safeFileName = Path.GetFileName(nome);
        var localFilePath = Path.Combine(localDir, safeFileName);

        using (var fileStream = File.Create(localFilePath))
        {
            if (arquivo.CanSeek)
                arquivo.Position = 0;

            await arquivo.CopyToAsync(fileStream, cancellationToken);
        }

        _logger.LogInformation("Arquivo salvo localmente em: {Caminho}", localFilePath);
        return $"file:///{localFilePath.Replace('\\', '/')}";
    }
}
