using InterfacesComProposito.Application.Ports.Notifications;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Notifications;

/// <summary>
/// Adapter concreto para envio de notificações por E-mail.
/// 
/// NOTA ARQUITETURAL:
/// A porta INotificador permite que o caso de uso envie comunicações sem conhecer
/// protocolos de transporte (SMTP, SendGrid, Amazon SES, etc.).
/// </summary>
public sealed class NotificadorEmail : INotificador
{
    private readonly ILogger<NotificadorEmail> _logger;

    public NotificadorEmail(ILogger<NotificadorEmail> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task NotificarAsync(Notificacao notificacao, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notificacao);

        _logger.LogInformation("[E-MAIL ENVIADO] Para: {Destinatario} | Assunto: {Assunto} | Mensagem: {Mensagem}",
            notificacao.Destinatario,
            notificacao.Assunto,
            notificacao.Mensagem);

        return Task.CompletedTask;
    }
}
