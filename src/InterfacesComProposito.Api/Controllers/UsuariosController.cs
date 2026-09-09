using InterfacesComProposito.Api.Models;
using InterfacesComProposito.Application.Usuarios;
using InterfacesComProposito.Application.Usuarios.ConsultarUsuario;
using InterfacesComProposito.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace InterfacesComProposito.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController : ControllerBase
{
    private readonly ConsultarUsuarioHandler _consultarUsuarioHandler;
    private readonly UsuarioService _usuarioService;

    public UsuariosController(
        ConsultarUsuarioHandler consultarUsuarioHandler,
        UsuarioService usuarioService)
    {
        _consultarUsuarioHandler = consultarUsuarioHandler ?? throw new ArgumentNullException(nameof(consultarUsuarioHandler));
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
    }

    /// <summary>
    /// Consulta os dados de um usuário pelo seu identificador.
    /// Fluxo demonstrado no artigo: Controller -> ConsultarUsuarioHandler -> UsuarioService -> IRepositorioUsuarios
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConsultarUsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _consultarUsuarioHandler.ExecutarAsync(id, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Cadastra um novo usuário utilizando diretamente o serviço interno concreto UsuarioService.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        Endereco? endereco = null;
        if (request.Endereco is not null)
        {
            endereco = new Endereco(
                request.Endereco.Logradouro,
                request.Endereco.Numero,
                request.Endereco.Complemento,
                request.Endereco.Bairro,
                request.Endereco.Cidade,
                request.Endereco.Estado,
                request.Endereco.Cep);
        }

        var usuario = await _usuarioService.CriarUsuarioAsync(
            request.Nome,
            request.Email,
            endereco,
            cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Ativo,
            usuario.CriadoEm
        });
    }
}
