using InterfacesComProposito.Domain.Enums;
using InterfacesComProposito.Domain.Exceptions;

namespace InterfacesComProposito.Domain.Entities;

public sealed class Pagamento
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public decimal Valor { get; private set; }
    public StatusPagamento Status { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset? ProcessadoEm { get; private set; }

    public Pagamento(Guid pedidoId, decimal valor)
    {
        if (pedidoId == Guid.Empty)
            throw new RegraDeNegocioException("Identificador de pedido inválido para pagamento.");

        if (valor <= 0)
            throw new RegraDeNegocioException("O valor do pagamento deve ser maior que zero.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        Valor = valor;
        Status = StatusPagamento.Pendente;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    private Pagamento()
    {
    }

    public void IniciarProcessamento()
    {
        if (Status != StatusPagamento.Pendente)
            throw new RegraDeNegocioException("Apenas pagamentos pendentes podem iniciar processamento.");

        Status = StatusPagamento.Processando;
    }

    public void Aprovar()
    {
        if (Status == StatusPagamento.Aprovado)
            return;

        Status = StatusPagamento.Aprovado;
        ProcessadoEm = DateTimeOffset.UtcNow;
    }

    public void Recusar()
    {
        if (Status == StatusPagamento.Aprovado)
            throw new RegraDeNegocioException("Não é possível recusar um pagamento que já foi aprovado.");

        Status = StatusPagamento.Recusado;
        ProcessadoEm = DateTimeOffset.UtcNow;
    }
}
