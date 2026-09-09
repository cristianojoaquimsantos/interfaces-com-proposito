namespace InterfacesComProposito.Application.Usuarios.ConsultarUsuario;

/// <summary>
/// Handler concreto para o caso de uso de consulta de usuário.
/// 
/// NOTA ARQUITETURAL:
/// Depende diretamente da classe concreta UsuarioService.
/// Ambos pertencem à camada Application. Não existe intermediário ou interface artificial (IConsultarUsuarioHandler).
/// Demonstra a comunicação concreta entre casos de uso e serviços de domínio da mesma camada.
/// </summary>
public sealed class ConsultarUsuarioHandler
{
    private readonly UsuarioService _usuarioService;

    public ConsultarUsuarioHandler(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
    }

    public async Task<ConsultarUsuarioDto> ExecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.ObterUsuarioValidoAsync(id, cancellationToken);

        var enderecoFormatado = usuario.Endereco is not null
            ? $"{usuario.Endereco.Logradouro}, {usuario.Endereco.Numero} - {usuario.Endereco.Bairro}, {usuario.Endereco.Cidade}/{usuario.Endereco.Estado}"
            : null;

        return new ConsultarUsuarioDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Ativo,
            usuario.CriadoEm,
            enderecoFormatado);
    }
}
