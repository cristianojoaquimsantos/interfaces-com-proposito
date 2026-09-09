using InterfacesComProposito.Application.Ports.Audit;
using InterfacesComProposito.Application.Ports.Messaging;
using InterfacesComProposito.Application.Ports.Notifications;
using InterfacesComProposito.Application.Ports.Payments;
using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Application.Ports.Security;
using InterfacesComProposito.Application.Ports.Storage;
using InterfacesComProposito.Infrastructure.Audit;
using InterfacesComProposito.Infrastructure.Data;
using InterfacesComProposito.Infrastructure.Messaging;
using InterfacesComProposito.Infrastructure.Notifications;
using InterfacesComProposito.Infrastructure.Payments;
using InterfacesComProposito.Infrastructure.Persistence;
using InterfacesComProposito.Infrastructure.Security;
using InterfacesComProposito.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterfacesComProposito.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // -----------------------------------------------------------------------------------
            // PERSISTÊNCIA: EF CORE COM SQL SERVER
            // -----------------------------------------------------------------------------------
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=InterfacesComPropositoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            });

            // -----------------------------------------------------------------------------------
            // FRONTEIRAS ARQUITETURAIS: ABSTRAÇÕES + ADAPTERS CONCRETOS
            // -----------------------------------------------------------------------------------
            // CONTRASTE INTENCIONAL:
            // Enquanto a Application registrou classes concretas (UsuarioService, Handlers),
            // aqui na Infrastructure TODAS as dependências são registradas associando
            // a abstração (porta da Application) ao adapter concreto (tecnologia de Infrastructure).
            //
            // Quem examina o DI percebe imediatamente as fronteiras do sistema:
            // - Persistência (SQL Server / EF Core)
            // - Armazenamento externo (Azure Blob Storage)
            // - Mensageria externa (Azure Service Bus)
            // - Gestão de segredos (Azure Key Vault)
            // - Pagamento externo (Gateway)
            // - Notificação externa (E-mail)
            // - Auditoria externa (Log estruturado)
            // -----------------------------------------------------------------------------------
            services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosSqlServer>();
            services.AddScoped<IRepositorioPedidos, RepositorioPedidosSqlServer>();
            services.AddScoped<IRepositorioProdutos, RepositorioProdutosSqlServer>();

            services.AddScoped<IFileStorage, AzureBlobFileStorage>();
            services.AddScoped<IMessagePublisher, AzureServiceBusPublisher>();
            services.AddScoped<ISecretProvider, AzureKeyVaultSecretProvider>();

            services.AddScoped<IPagamentoGateway, PagamentoGatewaySimulado>();
            services.AddScoped<INotificador, NotificadorEmail>();
            services.AddScoped<IAuditoria, AuditoriaLog>();

            return services;
        }
    }
}
