using InterfacesComProposito.Application.Ports.Audit;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Audit;

/// <summary>
/// Adapter concreto para registro de eventos de auditoria em logs estruturados.
/// 
/// NOTA ARQUITETURAL:
/// O nome desta classe e de sua porta (IAuditoria) rejeitam a convenção cega de sufixar tudo com "Service".
/// Atende à capacidade de registrar auditoria preservando semântica clara de domínio e infraestrutura.
/// </summary>
public sealed class AuditoriaLog : IAuditoria
{
    private readonly ILogger<AuditoriaLog> _logger;

    public AuditoriaLog(ILogger<AuditoriaLog> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task RegistrarAsync(EventoAuditoria evento, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evento);

        _logger.LogInformation("[AUDITORIA] [{Timestamp:u}] Operação: {Operacao} | Detalhes: {Detalhes}",
            evento.Timestamp,
            evento.Operacao,
            evento.Detalhes);

        return Task.CompletedTask;
    }
}
