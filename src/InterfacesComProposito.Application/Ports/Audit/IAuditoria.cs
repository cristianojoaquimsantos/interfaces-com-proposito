namespace InterfacesComProposito.Application.Ports.Audit;

public sealed record EventoAuditoria(
    string Operacao,
    string Detalhes,
    DateTimeOffset Timestamp);

/// <summary>
/// Porta semântica de auditoria corporativa.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Fronteira arquitetural para registro de trilha de auditoria externa ou de conformidade regulatória.
/// O nome expressa a capacidade/conceito de negócio ("IAuditoria"), e não um sufixo artificial "IAuditoriaService".
/// Permite enviar eventos para sistemas de observabilidade, SIEM ou data lake sem poluir os casos de uso.
/// </summary>
public interface IAuditoria
{
    Task RegistrarAsync(
        EventoAuditoria evento,
        CancellationToken cancellationToken);
}
