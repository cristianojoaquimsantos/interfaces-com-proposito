using InterfacesComProposito.Api.Middleware;
using InterfacesComProposito.Infrastructure.Data;
using InterfacesComProposito.IoC;
// Necessário para testes de integração com WebApplicationFactory<Program>

public partial class Program {
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuração de Controllers e OpenAPI nativo do .NET 10
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // Tratamento centralizado de erros via ProblemDetails (RFC 7807)
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // -----------------------------------------------------------------------------------
        // COMPOSITION ROOT (CENTRALIZAÇÃO DA COMPOSIÇÃO DA ARQUITETURA)
        // -----------------------------------------------------------------------------------
        // Application: registra componentes internos concretos (UsuarioService, Handlers)
        // e contratos públicos (ICalculadoraDePreco).
        // Infrastructure: registra adaptadores de persistência (SQL Server), mensageria (Service Bus),
        // storage (Blob Storage), pagamentos, notificações e auditoria vinculados às suas Ports.
        builder.Services.AddDependencyInjection(builder.Configuration);

        var app = builder.Build();

        // Inicialização e carga inicial de dados para execução e testes imediatos
        await DbInitializer.InicializarAsync(app.Services);

        // Middleware Pipeline
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

// Necessário para testes de integração com WebApplicationFactory<Program>
public partial class Program { }
