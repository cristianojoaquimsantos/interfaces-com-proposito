namespace InterfacesComProposito.Application.Ports.Messaging;

/// <summary>
/// Porta de publicação assíncrona de mensagens e eventos.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Isolamento de mensageria e capacidade de mensageria externa.
/// A camada Application precisa notificar o ecossistema distribuído quando eventos relevantes ocorrem (ex: PedidoFinalizado),
/// sem conhecer a tecnologia de mensageria concreta (Azure Service Bus, RabbitMQ, Kafka, etc.).
/// O contrato expressa uma CAPACIDADE ("publicar mensagem"), não uma tecnologia.
/// </summary>
public interface IMessagePublisher
{
    Task PublicarAsync<T>(
        T mensagem,
        CancellationToken cancellationToken);
}
