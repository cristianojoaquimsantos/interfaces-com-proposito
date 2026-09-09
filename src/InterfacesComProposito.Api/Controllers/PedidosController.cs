using InterfacesComProposito.Api.Models;
using InterfacesComProposito.Application.Pedidos.AdicionarItemPedido;
using InterfacesComProposito.Application.Pedidos.AnexarComprovantePedido;
using InterfacesComProposito.Application.Pedidos.ConsultarPedido;
using InterfacesComProposito.Application.Pedidos.CriarPedido;
using InterfacesComProposito.Application.Pedidos.FinalizarPedido;
using Microsoft.AspNetCore.Mvc;

namespace InterfacesComProposito.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PedidosController : ControllerBase
{
    private readonly CriarPedidoHandler _criarPedidoHandler;
    private readonly ConsultarPedidoHandler _consultarPedidoHandler;
    private readonly AdicionarItemPedidoHandler _adicionarItemPedidoHandler;
    private readonly FinalizarPedidoHandler _finalizarPedidoHandler;
    private readonly AnexarComprovantePedidoHandler _anexarComprovantePedidoHandler;

    public PedidosController(
        CriarPedidoHandler criarPedidoHandler,
        ConsultarPedidoHandler consultarPedidoHandler,
        AdicionarItemPedidoHandler adicionarItemPedidoHandler,
        FinalizarPedidoHandler finalizarPedidoHandler,
        AnexarComprovantePedidoHandler anexarComprovantePedidoHandler)
    {
        _criarPedidoHandler = criarPedidoHandler ?? throw new ArgumentNullException(nameof(criarPedidoHandler));
        _consultarPedidoHandler = consultarPedidoHandler ?? throw new ArgumentNullException(nameof(consultarPedidoHandler));
        _adicionarItemPedidoHandler = adicionarItemPedidoHandler ?? throw new ArgumentNullException(nameof(adicionarItemPedidoHandler));
        _finalizarPedidoHandler = finalizarPedidoHandler ?? throw new ArgumentNullException(nameof(finalizarPedidoHandler));
        _anexarComprovantePedidoHandler = anexarComprovantePedidoHandler ?? throw new ArgumentNullException(nameof(anexarComprovantePedidoHandler));
    }

    /// <summary>
    /// Cria um novo pedido com validação de usuário e catálogo de produtos.
    /// Fluxo: Controller -> CriarPedidoHandler -> UsuarioService -> IRepositorioUsuarios / IRepositorioPedidos
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CriarPedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CriarPedidoCommand(
            request.UsuarioId,
            request.Itens.Select(i => new ItemPedidoCommand(i.ProdutoId, i.Quantidade)).ToList());

        var resultado = await _criarPedidoHandler.ExecutarAsync(command, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.PedidoId }, resultado);
    }

    /// <summary>
    /// Consulta o estado completo de um pedido pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _consultarPedidoHandler.ExecutarAsync(id, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Adiciona um novo item ao pedido existente, recalculando totais e validando estado.
    /// </summary>
    [HttpPost("{id:guid}/itens")]
    [ProducesResponseType(typeof(AdicionarItemPedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AdicionarItem(
        Guid id,
        [FromBody] AdicionarItemPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AdicionarItemPedidoCommand(id, request.ProdutoId, request.Quantidade);
        var resultado = await _adicionarItemPedidoHandler.ExecutarAsync(command, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Finaliza o pedido, processa o pagamento via gateway externo e publica evento no Service Bus.
    /// </summary>
    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(typeof(FinalizarPedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finalizar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new FinalizarPedidoCommand(id);
        var resultado = await _finalizarPedidoHandler.ExecutarAsync(command, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Anexa arquivo de comprovante ao pedido utilizando a porta IFileStorage (Azure Blob Storage).
    /// </summary>
    [HttpPost("{id:guid}/comprovante")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AnexarComprovantePedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnexarComprovante(
        Guid id,
        IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Arquivo inválido",
                Detail = "Nenhum arquivo enviado para o comprovante."
            });

        using var stream = arquivo.OpenReadStream();
        var command = new AnexarComprovantePedidoCommand(id, stream, arquivo.FileName);

        var resultado = await _anexarComprovantePedidoHandler.ExecutarAsync(command, cancellationToken);
        return Ok(resultado);
    }
}
