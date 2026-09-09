using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Application.Ports.Storage;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Application.Pedidos.AnexarComprovantePedido;

/// <summary>
/// Handler concreto para upload e anexo de comprovante do pedido.
/// 
/// NOTA ARQUITETURAL:
/// Depende da porta IFileStorage para isolar a tecnologia de armazenamento (Azure Blob Storage).
/// A Application não sabe se os bytes vão para a nuvem da Microsoft, AWS ou um disco local:
/// ela lida apenas com Streams e a URL retornada pela porta.
/// </summary>
public sealed class AnexarComprovantePedidoHandler
{
    private readonly IRepositorioPedidos _repositorioPedidos;
    private readonly IFileStorage _fileStorage;

    public AnexarComprovantePedidoHandler(
        IRepositorioPedidos repositorioPedidos,
        IFileStorage fileStorage)
    {
        _repositorioPedidos = repositorioPedidos ?? throw new ArgumentNullException(nameof(repositorioPedidos));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
    }

    public async Task<AnexarComprovantePedidoResponse> ExecutarAsync(AnexarComprovantePedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await _repositorioPedidos.ObterAsync(command.PedidoId, cancellationToken);
        if (pedido is null)
            throw RecursoNaoEncontradoException.Para<Pedido>(command.PedidoId);

        // 1. Salva arquivo através da porta IFileStorage
        var nomeFormatado = $"comprovantes/{pedido.Id}_{Guid.NewGuid()}_{command.NomeArquivo}";
        var urlComprovante = await _fileStorage.SalvarAsync(command.ArquivoStream, nomeFormatado, cancellationToken);

        // 2. Anexa ao pedido pelo método do Domínio
        pedido.AnexarComprovante(urlComprovante);

        // 3. Persiste
        await _repositorioPedidos.SalvarAsync(pedido, cancellationToken);

        return new AnexarComprovantePedidoResponse(pedido.Id, urlComprovante);
    }
}
