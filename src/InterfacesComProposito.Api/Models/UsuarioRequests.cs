namespace InterfacesComProposito.Api.Models;

public sealed record EnderecoRequest(
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Cep);

public sealed record CriarUsuarioRequest(
    string Nome,
    string Email,
    EnderecoRequest? Endereco);
