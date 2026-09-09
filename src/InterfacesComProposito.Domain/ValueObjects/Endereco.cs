using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.ValueObjects;

public sealed record Endereco
{
    public string Logradouro { get; }
    public string Numero { get; }
    public string? Complemento { get; }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Estado { get; }
    public string Cep { get; }

    public Endereco(
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado,
        string cep)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            throw new RegraDeNegocioException("Logradouro é obrigatório.");

        if (string.IsNullOrWhiteSpace(numero))
            throw new RegraDeNegocioException("Número é obrigatório.");

        if (string.IsNullOrWhiteSpace(bairro))
            throw new RegraDeNegocioException("Bairro é obrigatório.");

        if (string.IsNullOrWhiteSpace(cidade))
            throw new RegraDeNegocioException("Cidade é obrigatória.");

        if (string.IsNullOrWhiteSpace(estado) || estado.Trim().Length != 2)
            throw new RegraDeNegocioException("Estado deve ser uma sigla válida de 2 caracteres (UF).");

        if (string.IsNullOrWhiteSpace(cep))
            throw new RegraDeNegocioException("CEP é obrigatório.");

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = complemento?.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpperInvariant();
        Cep = cep.Trim();
    }

    // Construtor protegido para uso do EF Core
    private Endereco()
    {
        Logradouro = string.Empty;
        Numero = string.Empty;
        Bairro = string.Empty;
        Cidade = string.Empty;
        Estado = string.Empty;
        Cep = string.Empty;
    }
}
