using System.Text.Json;
using Azure.Messaging.ServiceBus;
using InterfacesComProposito.Application.Ports.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Messaging;

/// <summary>
/// Implementação de IMessagePublisher utilizando o SDK oficial do Azure Service Bus.
/// 
/// NOTA ARQUITETURAL:
/// A porta IMessagePublisher representa a capacidade de publicar eventos.
/// Esta classe é o adapter concreto que traduz o objeto genérico T para ServiceBusMessage e publica no broker.
/// A Application não sabe o que é um ServiceBusSender nem qual é a connection string.
/// </summary>
public sealed class AzureServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient? _client;
    private readonly ServiceBusSender? _sender;
    private readonly ILogger<AzureServiceBusPublisher> _logger;
    private readonly bool _useAzure;

    public AzureServiceBusPublisher(IConfiguration configuration, ILogger<AzureServiceBusPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var connectionString = configuration["Azure:ServiceBus:ConnectionString"];
        var topicOrQueueName = configuration["Azure:ServiceBus:TopicName"] ?? "pedidos-finalizados";

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains("YOUR_"))
        {
            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(topicOrQueueName);
            _useAzure = true;
        }
        else
        {
            _useAzure = false;
            _logger.LogInformation("Azure Service Bus não configurado com credenciais válidas. Utilizando publicação simulada em log para execução local.");
        }
    }

    public async Task PublicarAsync<T>(T mensagem, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mensagem);

        var json = JsonSerializer.Serialize(mensagem);

        if (_useAzure && _sender is not null)
        {
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = typeof(T).Name
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
            _logger.LogInformation("Mensagem {Tipo} publicada no Azure Service Bus.", typeof(T).Name);
            return;
        }

        // Execução local autônoma (log de emissão da mensagem)
        _logger.LogInformation("[LOCAL EVENT DISPATCHED] Tipo: {Tipo} | Conteúdo: {Payload}", typeof(T).Name, json);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender is not null)
            await _sender.DisposeAsync();

        if (_client is not null)
            await _client.DisposeAsync();
    }
}
