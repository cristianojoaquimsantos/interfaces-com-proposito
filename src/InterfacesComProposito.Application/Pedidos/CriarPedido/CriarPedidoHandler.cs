using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Application.Usuarios;
using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.Exceptions;
using InterfacesComProposito.Pricing.Contracts;

namespace InterfacesComProposito.Application.Pedidos.CriarPedido;

/// <summary>
/// Caso de uso concreto para criação de pedidos.
/// 
/// NOTA ARQUITETURAL:
/// Este handler demonstra a convivência entre:
/// - Uma dependência concreta interna: UsuarioService (lógica da mesma camada, sem interface).
/// - Abstrações arquiteturais reais: IRepositorioProdutos e IRepositorioPedidos (Ports para DIP com EF Core).
/// - Um contrato público compartilhável: ICalculadoraDePreco.
/// 
/// Não existe "ICriarPedidoHandler", pois isso seria mera duplicação de um caso de uso específico.
/// </summary>
public sealed class CriarPedidoHandler
{
    private readonly UsuarioService _usuarioService;
    private readonly IRepositorioProdutos _repositorioProdutos;
    private readonly IRepositorioPedidos _repositorioPedidos;
    private readonly ICalculadoraDePreco _calculadoraDePreco;

    public CriarPedidoHandler(
        UsuarioService usuarioService,
        IRepositorioProdutos repositorioProdutos,
        IRepositorioPedidos repositorioPedidos,
        ICalculadoraDePreco calculadoraDePreco)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _repositorioProdutos = repositorioProdutos ?? throw new ArgumentNullException(nameof(repositorioProdutos));
        _repositorioPedidos = repositorioPedidos ?? throw new ArgumentNullException(nameof(repositorioPedidos));
        _calculadoraDePreco = calculadoraDePreco ?? throw new ArgumentNullException(nameof(calculadoraDePreco));
    }

    public async Task<CriarPedidoResponse> ExecutarAsync(CriarPedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Itens is null || command.Itens.Count == 0)
            throw new DomainException("O pedido deve conter ao menos um item.");

        // 1. Valida o usuário através do serviço interno concreto (garante que existe e está ativo)
        var usuario = await _usuarioService.ObterUsuarioValidoAsync(command.UsuarioId, cancellationToken);

        // 2. Cria a raiz de agregação do Domínio
        var pedido = new Pedido(usuario.Id);

        // 3. Obtém produtos do catálogo através da porta de persistência
        var produtoIds = command.Itens.Select(i => i.ProdutoId).Distinct();
        var produtos = await _repositorioProdutos.ObterPorIdsAsync(produtoIds, cancellationToken);
        var mapaProdutos = produtos.ToDictionary(p => p.Id);

        // 4. Adiciona itens ao pedido protegendo as invariantes do Domínio
        var itensParaCalculo = new List<ItemParaCalculo>();

        foreach (var itemCmd in command.Itens)
        {
            if (!mapaProdutos.TryGetValue(itemCmd.ProdutoId, out var produto))
                throw RecursoNaoEncontradoException.Para<Produto>(itemCmd.ProdutoId);

            produto.GarantirDisponivelParaVenda();

            pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco, itemCmd.Quantidade);

            itensParaCalculo.Add(new ItemParaCalculo(
                produto.Id,
                produto.Nome,
                produto.Preco,
                itemCmd.Quantidade));
        }

        // 5. Utiliza o contrato público de precificação para validar o cálculo
        var pedidoParaCalculo = new PedidoParaCalculo(pedido.Id, pedido.UsuarioId, itensParaCalculo);
        _ = await _calculadoraDePreco.CalcularAsync(pedidoParaCalculo, cancellationToken);

        // 6. Persiste via porta de persistência
        await _repositorioPedidos.AdicionarAsync(pedido, cancellationToken);

        return new CriarPedidoResponse(
            pedido.Id,
            pedido.ValorTotal,
            pedido.Status.ToString());
    }
}
