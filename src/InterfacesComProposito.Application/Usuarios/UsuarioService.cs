using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;
using InterfacesComProposito.Domain.ValueObjects;

namespace InterfacesComProposito.Application.Usuarios;

/// <summary>
/// Serviço interno da camada Application para orquestração de regras relacionadas a Usuários.
/// 
/// DECISÃO ARQUITETURAL CRÍTICA:
/// Esta classe PROPOSITAMENTE NÃO POSSUI uma interface "IUsuarioService".
/// 
/// Por que NÃO criar IUsuarioService?
/// 1. Não existe outra implementação concebível dentro do sistema.
/// 2. Não existe fronteira tecnológica a ser isolada (o código é 100% C# gerenciado na mesma camada).
/// 3. Não há inversão de dependência (DIP) necessária: tanto quem chama (Handlers) quanto UsuarioService
///    estão dentro da camada Application.
/// 4. Não é um contrato público exposto para terceiros.
/// 5. Dependency Injection NÃO EXIGE interface: classes concretas podem e devem ser registradas
///    diretamente no DI Container (ex: services.AddScoped<UsuarioService>()).
/// 
/// Criar "IUsuarioService" aqui seria apenas cerimônia vazia, duplicação de código e burocracia desprovida de valor arquitetural.
/// </summary>
public sealed class UsuarioService
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;

    public UsuarioService(IRepositorioUsuarios repositorioUsuarios)
    {
        _repositorioUsuarios = repositorioUsuarios ?? throw new ArgumentNullException(nameof(repositorioUsuarios));
    }

    public async Task<Usuario> ObterUsuarioValidoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _repositorioUsuarios.ObterAsync(usuarioId, cancellationToken);
        if (usuario is null)
            throw RecursoNaoEncontradoException.Para<Usuario>(usuarioId);

        usuario.GarantirAtivo();

        return usuario;
    }

    public async Task<Usuario> CriarUsuarioAsync(string nome, string email, Endereco? endereco, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(nome, email, endereco);
        await _repositorioUsuarios.AdicionarAsync(usuario, cancellationToken);
        return usuario;
    }

    public async Task InativarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await ObterUsuarioValidoAsync(usuarioId, cancellationToken);
        usuario.Inativar();
        await _repositorioUsuarios.SalvarAsync(usuario, cancellationToken);
    }
}
