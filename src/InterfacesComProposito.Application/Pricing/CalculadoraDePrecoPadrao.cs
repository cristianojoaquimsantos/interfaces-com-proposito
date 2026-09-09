using InterfacesComProposito.Pricing.Contracts;

namespace InterfacesComProposito.Application.Pricing;

/// <summary>
/// Implementação concreta do contrato público ICalculadoraDePreco.
/// 
/// NOTA ARQUITETURAL:
/// Esta é propositalmente a ÚNICA implementação desta interface na solução.
/// A existência de ICalculadoraDePreco não se apoia na multiplicidade de implementações,
/// mas sim no fato de ser um CONTRATO PÚBLICO fornecido para consumo externo.
/// Consumidores externos dependem apenas de ICalculadoraDePreco e dos DTOs de Pricing.Contracts,
/// enquanto esta implementação pode evoluir regras de cálculo e otimizações internamente.
/// </summary>
public sealed class CalculadoraDePrecoPadrao : ICalculadoraDePreco
{
    public Task<decimal> CalcularAsync(
        PedidoParaCalculo pedido,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        // Lógica de cálculo com soma dos itens
        var total = pedido.Itens.Sum(item => item.PrecoUnitario * item.Quantidade);

        return Task.FromResult(total);
    }
}
