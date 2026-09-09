using InterfacesComProposito.Domain.Exceptions;
using InterfacesComProposito.Domain.ValueObjects;

namespace InterfacesComProposito.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public bool Ativo { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public Endereco? Endereco { get; private set; }

    public Usuario(string nome, string email, Endereco? endereco = null)
    {
        ValidarNome(nome);
        ValidarEmail(email);

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Ativo = true;
        CriadoEm = DateTimeOffset.UtcNow;
        Endereco = endereco;
    }

    // Construtor protegido para o EF Core
    private Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
    }

    public void Ativar()
    {
        if (Ativo)
            return;

        Ativo = true;
    }

    public void Inativar()
    {
        if (!Ativo)
            return;

        Ativo = false;
    }

    public void AtualizarDados(string nome, string email)
    {
        GarantirAtivo();
        ValidarNome(nome);
        ValidarEmail(email);

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public void AtualizarEndereco(Endereco endereco)
    {
        GarantirAtivo();
        Endereco = endereco ?? throw new RegraDeNegocioException("Endereço não pode ser nulo.");
    }

    public void GarantirAtivo()
    {
        if (!Ativo)
            throw new RegraDeNegocioException($"O usuário '{Nome}' está inativo e não pode realizar operações.");
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new RegraDeNegocioException("Nome do usuário deve conter ao menos 3 caracteres.");
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            throw new RegraDeNegocioException("E-mail informado é inválido.");
    }
}
