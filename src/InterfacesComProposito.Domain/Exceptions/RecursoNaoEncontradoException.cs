namespace InterfacesComProposito.Domain.Exceptions;

public sealed class RecursoNaoEncontradoException : DomainException
{
    public RecursoNaoEncontradoException(string message) : base(message)
    {
    }

    public static RecursoNaoEncontradoException Para<T>(Guid id) =>
        new($"{typeof(T).Name} com identificador '{id}' não foi encontrado(a).");
}
