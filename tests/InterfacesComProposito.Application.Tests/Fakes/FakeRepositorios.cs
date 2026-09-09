using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Domain.Entities;

namespace InterfacesComProposito.Application.Tests.Fakes;

public sealed class FakeRepositorioUsuarios : IRepositorioUsuarios
{
    private readonly Dictionary<Guid, Usuario> _usuarios = [];

    public Task<Usuario?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        _usuarios.TryGetValue(id, out var usuario);
        return Task.FromResult(usuario);
    }

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        _usuarios[usuario.Id] = usuario;
        return Task.CompletedTask;
    }

    public Task SalvarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        _usuarios[usuario.Id] = usuario;
        return Task.CompletedTask;
    }
}

public sealed class FakeRepositorioPedidos : IRepositorioPedidos
{
    private readonly Dictionary<Guid, Pedido> _pedidos = [];

    public Task<Pedido?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        _pedidos.TryGetValue(id, out var pedido);
        return Task.FromResult(pedido);
    }

    public Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        _pedidos[pedido.Id] = pedido;
        return Task.CompletedTask;
    }

    public Task SalvarAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        _pedidos[pedido.Id] = pedido;
        return Task.CompletedTask;
    }
}

public sealed class FakeRepositorioProdutos : IRepositorioProdutos
{
    private readonly Dictionary<Guid, Produto> _produtos = [];

    public void AdicionarParaTeste(Produto produto)
    {
        _produtos[produto.Id] = produto;
    }

    public Task<Produto?> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        _produtos.TryGetValue(id, out var produto);
        return Task.FromResult(produto);
    }

    public Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        var resultado = _produtos.Values.Where(p => idList.Contains(p.Id)).ToList();
        return Task.FromResult<IReadOnlyList<Produto>>(resultado);
    }

    public Task AdicionarAsync(Produto produto, CancellationToken cancellationToken)
    {
        _produtos[produto.Id] = produto;
        return Task.CompletedTask;
    }
}
