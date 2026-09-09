using InterfacesComProposito.Application.Pedidos.AdicionarItemPedido;
using InterfacesComProposito.Application.Pedidos.AnexarComprovantePedido;
using InterfacesComProposito.Application.Pedidos.ConsultarPedido;
using InterfacesComProposito.Application.Pedidos.CriarPedido;
using InterfacesComProposito.Application.Pedidos.FinalizarPedido;
using InterfacesComProposito.Application.Ports.Audit;
using InterfacesComProposito.Application.Ports.Messaging;
using InterfacesComProposito.Application.Ports.Notifications;
using InterfacesComProposito.Application.Ports.Payments;
using InterfacesComProposito.Application.Ports.Persistence;
using InterfacesComProposito.Application.Ports.Security;
using InterfacesComProposito.Application.Ports.Storage;
using InterfacesComProposito.Application.Pricing;
using InterfacesComProposito.Application.Usuarios;
using InterfacesComProposito.Application.Usuarios.ConsultarUsuario;
using InterfacesComProposito.Infrastructure.Audit;
using InterfacesComProposito.Infrastructure.Data;
using InterfacesComProposito.Infrastructure.Messaging;
using InterfacesComProposito.Infrastructure.Notifications;
using InterfacesComProposito.Infrastructure.Payments;
using InterfacesComProposito.Infrastructure.Persistence;
using InterfacesComProposito.Infrastructure.Security;
using InterfacesComProposito.Infrastructure.Storage;
using InterfacesComProposito.Pricing.Contracts;
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

            // -----------------------------------------------------------------------------------
            // COMPONENTES INTERNOS: REGISTRADOS DIRETAMENTE COMO CLASSES CONCRETAS
            // -----------------------------------------------------------------------------------
            // DECISÃO ARQUITETURAL:
            // UsuarioService e os Handlers de casos de uso NÃO possuem interfaces.
            // Eles representam lógica interna da camada Application.
            // Não há necessidade de DIP, não há fronteiras tecnológicas e não existem múltiplas
            // implementações concorrentes.
            //
            // O container de DI resolve classes concretas de forma nativa e eficiente.
            // Dependency Injection != obrigação de usar interface.
            // -----------------------------------------------------------------------------------
            services.AddScoped<UsuarioService>();
            services.AddScoped<ConsultarUsuarioHandler>();
            services.AddScoped<CriarPedidoHandler>();
            services.AddScoped<ConsultarPedidoHandler>();
            services.AddScoped<AdicionarItemPedidoHandler>();
            services.AddScoped<FinalizarPedidoHandler>();
            services.AddScoped<AnexarComprovantePedidoHandler>();

            // -----------------------------------------------------------------------------------
            // CONTRATO PÚBLICO COMPARTILHÁVEL: INTERFACE COM IMPLEMENTAÇÃO ÚNICA
            // -----------------------------------------------------------------------------------
            // ICalculadoraDePreco existe porque representa uma API pública distribuível para
            // consumidores externos, justificando o desacoplamento da interface em relação à classe.
            // -----------------------------------------------------------------------------------
            services.AddScoped<ICalculadoraDePreco, CalculadoraDePrecoPadrao>();

            return services;
        }
    }
}
