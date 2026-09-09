using InterfacesComProposito.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InterfacesComProposito.Api.Middleware;

/// <summary>
/// Tratamento centralizado de erros em conformidade com RFC 7807 (ProblemDetails).
/// 
/// NOTA ARQUITETURAL:
/// Remove a necessidade de blocos try/catch repetitivos em Controllers.
/// Mapeia exceções de domínio e negócio diretamente para códigos HTTP semânticos:
/// - 404: RecursoNaoEncontradoException
/// - 409: RegraDeNegocioException (conflito com o estado/invariante do domínio)
/// - 400: DomainException / ArgumentException (parâmetros ou validação de entrada inválida)
/// - 500: Erros não tratados de sistema
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exceção capturada pelo manipulador global: {Mensagem}", exception.Message);

        var (statusCode, title, type) = exception switch
        {
            RecursoNaoEncontradoException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4"),

            RegraDeNegocioException => (
                StatusCodes.Status409Conflict,
                "Violação de regra de negócio",
                "https://tools.ietf.org/html/rfc7231#section-6.5.8"),

            DomainException or ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação ou parâmetro inválido",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
