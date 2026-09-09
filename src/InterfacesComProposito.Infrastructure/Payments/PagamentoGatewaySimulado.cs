using InterfacesComProposito.Application.Ports.Payments;
using InterfacesComProposito.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Payments;

/// <summary>
/// Adapter simulado de Gateway de Pagamento.
/// 
/// NOTA ARQUITETURAL:
/// Permite testar e executar a aplicação localmente sem depender de adquirentes ou contratos com Cielo/Stripe reais.
/// A interface IPagamentoGateway é justificada porque o processamento de pagamentos representa uma dependência
/// externa com protocolo de rede, transacionalidade e adquirentes variáveis.
/// </summary>
public sealed class PagamentoGatewaySimulado : IPagamentoGateway
{
    private readonly ILogger<PagamentoGatewaySimulado> _logger;

    public PagamentoGatewaySimulado(ILogger<PagamentoGatewaySimulado> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ResultadoPagamento> ProcessarAsync(Pagamento pagamento, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pagamento);

        _logger.LogInformation("Processando pagamento no valor de {Valor:C2} para o Pedido {PedidoId}.", pagamento.Valor, pagamento.PedidoId);

        // Simulação: valores superiores a 1 milhão são recusados para fins de teste de cenários de erro
        if (pagamento.Valor > 1_000_000m)
        {
            _logger.LogWarning("Pagamento recusado: valor de {Valor:C2} excede limite de transação simulada.", pagamento.Valor);
            return Task.FromResult(new ResultadoPagamento(
                Sucesso: false,
                TransacaoId: null,
                Mensagem: "Limite financeiro excedido para a transação."));
        }

        var transacaoId = $"TX_{Guid.NewGuid():N}";
        _logger.LogInformation("Pagamento aprovado com sucesso! Transação: {TransacaoId}", transacaoId);

        return Task.FromResult(new ResultadoPagamento(
            Sucesso: true,
            TransacaoId: transacaoId,
            Mensagem: "Pagamento aprovado pelo gateway simulado."));
    }
}
