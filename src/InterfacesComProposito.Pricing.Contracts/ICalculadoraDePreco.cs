namespace InterfacesComProposito.Pricing.Contracts;

/// <summary>
/// Contrato público para cálculo de preços compartilhado entre diferentes sistemas e equipes.
/// 
/// JUSTIFICATIVA ARQUITETURAL:
/// Esta interface pode possuir apenas UMA ÚNICA implementação neste projeto (CalculadoraDePrecoPadrao).
/// Ainda assim, sua existência é plenamente justificada porque representa um CONTRATO PÚBLICO compartilhado
/// (pacote NuGet / biblioteca distribuída) entre múltiplos times ou microsserviços.
/// 
/// O ciclo de vida e a estabilidade da interface são independentes da implementação concreta,
/// permitindo que consumidores dependam apenas deste contrato desacoplado dos modelos internos da aplicação.
/// </summary>
public interface ICalculadoraDePreco
{
    Task<decimal> CalcularAsync(
        PedidoParaCalculo pedido,
        CancellationToken cancellationToken);
}
