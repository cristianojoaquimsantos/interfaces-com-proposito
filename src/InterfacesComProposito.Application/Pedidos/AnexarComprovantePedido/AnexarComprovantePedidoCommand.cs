namespace InterfacesComProposito.Application.Pedidos.AnexarComprovantePedido;

public sealed record AnexarComprovantePedidoCommand(
    Guid PedidoId,
    Stream ArquivoStream,
    string NomeArquivo);

public sealed record AnexarComprovantePedidoResponse(
    Guid PedidoId,
    string ComprovanteUrl);
