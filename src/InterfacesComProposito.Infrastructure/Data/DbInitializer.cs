using InterfacesComProposito.Domain.Entities;
using InterfacesComProposito.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InterfacesComProposito.Infrastructure.Data;

public static class DbInitializer
{
    public static readonly Guid UsuarioExemploId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ProdutoTecladoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProdutoMouseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ProdutoMonitorId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static async Task InicializarAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.EnsureCreatedAsync();

            if (!await context.Usuarios.AnyAsync())
            {
                var endereco = new Endereco(
                    "Avenida Paulista",
                    "1000",
                    "Apto 42",
                    "Bela Vista",
                    "São Paulo",
                    "SP",
                    "01310-100");

                var usuario = new Usuario("Desenvolvedor Sênior", "dev.senior@exemplo.com.br", endereco);

                // Forçar o Id fixo para facilitar testes e documentação
                typeof(Usuario).GetProperty(nameof(Usuario.Id))?.SetValue(usuario, UsuarioExemploId);

                await context.Usuarios.AddAsync(usuario);
            }

            if (!await context.Produtos.AnyAsync())
            {
                var produtos = new List<Produto>
                {
                    new(ProdutoTecladoId, "Teclado Mecânico ABNT2", 350.00m, ativo: true),
                    new(ProdutoMouseId, "Mouse Ergonômico Sem Fio", 180.00m, ativo: true),
                    new(ProdutoMonitorId, "Monitor 27 Polegadas 4K", 1950.00m, ativo: true)
                };

                await context.Produtos.AddRangeAsync(produtos);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Banco de dados inicializado e populado com dados de exemplo com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Aviso ao inicializar dados no banco (possível ausência temporária do SQL Server local).");
        }
    }
}
