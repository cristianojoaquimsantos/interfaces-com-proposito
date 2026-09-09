namespace InterfacesComProposito.Application.Usuarios.ConsultarUsuario;

public sealed record ConsultarUsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    bool Ativo,
    DateTimeOffset CriadoEm,
    string? EnderecoFormatado);
