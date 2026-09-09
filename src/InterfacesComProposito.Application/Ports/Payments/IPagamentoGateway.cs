using InterfacesComProposito.Domain.Entities;

namespace InterfacesComProposito.Application.Ports.Payments;

public sealed record ResultadoPagamento(
    bool Sucesso,
    string? TransacaoId,
    string? Mensagem);

/// <summary>
/// Porta para integração com Gateways de Pagamento externos.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Fronteira de integração externa. Processamento financeiro envolve transações com adquirentes
/// ou provedores externos (ex: Stripe, Cielo, MercadoPago, etc.).
/// O contrato isola a aplicação contra dependências externas de rede, protocolos proprietários e SDKs bancários.
/// </summary>
public interface IPagamentoGateway
{
    Task<ResultadoPagamento> ProcessarAsync(
        Pagamento pagamento,
        CancellationToken cancellationToken);
}
