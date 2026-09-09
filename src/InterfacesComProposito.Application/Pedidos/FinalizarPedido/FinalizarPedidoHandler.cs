using InterfacesComProposito.Application.Pedidos.Eventos;
using InterfacesComProposito.Application.Ports.Audit;
using InterfacesComProposito.Application.Ports.Messaging;
using InterfacesComProposito.Application.Ports.Notifications;
using InterfacesComProposito.Application.Ports.Payments;
using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Application.Pedidos.FinalizarPedido;

/// <summary>
/// Handler concreto para finalização de pedidos.
/// 
/// NOTA ARQUITETURAL:
/// Este handler orquestra o ciclo de vida final do pedido conectando exclusivamente
/// FRONTEIRAS ARQUITETURAIS REAIS através de suas respectivas portas:
/// - IRepositorioPedidos: Inversão de dependência para persistência do agregado.
/// - IPagamentoGateway: Integração financeira externa.
/// - IMessagePublisher: Mensageria distribuída assíncrona (Azure Service Bus).
/// - INotificador: Envio de notificação externa.
/// - IAuditoria: Registro semântico de auditoria.
/// 
/// Evitou-se o anti-pattern de inflar o construtor com interfaces artificiais
/// para serviços internos (como IEnderecoService, IUsuarioService, etc.).
/// </summary>
public sealed class FinalizarPedidoHandler
{
    private readonly IRepositorioPedidos _repositorioPedidos;
    private readonly IPagamentoGateway _pagamentoGateway;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificador _notificador;
    private readonly IAuditoria _auditoria;

    public FinalizarPedidoHandler(
        IRepositorioPedidos repositorioPedidos,
        IPagamentoGateway pagamentoGateway,
        IMessagePublisher messagePublisher,
        INotificador notificador,
        IAuditoria auditoria)
    {
        _repositorioPedidos = repositorioPedidos ?? throw new ArgumentNullException(nameof(repositorioPedidos));
        _pagamentoGateway = pagamentoGateway ?? throw new ArgumentNullException(nameof(pagamentoGateway));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
        _auditoria = auditoria ?? throw new ArgumentNullException(nameof(auditoria));
    }

    public async Task<FinalizarPedidoResponse> ExecutarAsync(FinalizarPedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. Recuperar o pedido
        var pedido = await _repositorioPedidos.ObterAsync(command.PedidoId, cancellationToken);
        if (pedido is null)
            throw RecursoNaoEncontradoException.Para<Pedido>(command.PedidoId);

        // 2. Finalizar usando método do Domínio (invariantes de pedido vazio, status, etc.)
        pedido.Finalizar();

        // 3. Processar pagamento via Gateway externo
        var pagamento = new Pagamento(pedido.Id, pedido.ValorTotal);
        pagamento.IniciarProcessamento();

        var resultadoPagamento = await _pagamentoGateway.ProcessarAsync(pagamento, cancellationToken);
        if (!resultadoPagamento.Sucesso)
        {
            pagamento.Recusar();
            throw new RegraDeNegocioException($"Pagamento recusado para o pedido {pedido.Id}: {resultadoPagamento.Mensagem}");
        }

        pagamento.Aprovar();

        // 4. Persistir a mudança de estado do Aggregate
        await _repositorioPedidos.SalvarAsync(pedido, cancellationToken);

        // 5. Publicar evento assíncrono para o ecossistema distribuído
        var eventoFinalizado = new PedidoFinalizadoEvent(
            pedido.Id,
            pedido.UsuarioId,
            pedido.ValorTotal,
            pedido.FinalizadoEm!.Value);

        await _messagePublisher.PublicarAsync(eventoFinalizado, cancellationToken);

        // 6. Notificação externa e auditoria
        await _notificador.NotificarAsync(new Notificacao(
            Destinatario: pedido.UsuarioId.ToString(),
            Assunto: $"Pedido {pedido.Id} finalizado com sucesso!",
            Mensagem: $"Seu pedido no valor de R$ {pedido.ValorTotal:F2} foi aprovado."), cancellationToken);

        await _auditoria.RegistrarAsync(new EventoAuditoria(
            Operacao: "FinalizarPedido",
            Detalhes: $"Pedido {pedido.Id} finalizado com transação {resultadoPagamento.TransacaoId}.",
            Timestamp: DateTimeOffset.UtcNow), cancellationToken);

        return new FinalizarPedidoResponse(
            pedido.Id,
            pedido.Status.ToString(),
            pedido.ValorTotal,
            pedido.FinalizadoEm.Value,
            resultadoPagamento.TransacaoId);
    }
}
