namespace InterfacesComProposito.Application.Ports.Notifications;

public sealed record Notificacao(
    string Destinatario,
    string Assunto,
    string Mensagem);

/// <summary>
/// Porta para despacho de notificações a canais externos (E-mail, SMS, Microsoft Teams, etc.).
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Isolamento de canal de comunicação externa.
/// O caso de uso precisa emitir alertas sem conhecer protocolos SMTP, Webhooks ou SDKs de mensageiros.
/// </summary>
public interface INotificador
{
    Task NotificarAsync(
        Notificacao notificacao,
        CancellationToken cancellationToken);
}
