using InterfacesComProposito.Application.Pedidos.AdicionarItemPedido;
using InterfacesComProposito.Application.Pedidos.AnexarComprovantePedido;
using InterfacesComProposito.Application.Pedidos.ConsultarPedido;
using InterfacesComProposito.Application.Pedidos.CriarPedido;
using InterfacesComProposito.Application.Pedidos.FinalizarPedido;
using InterfacesComProposito.Application.Pricing;
using InterfacesComProposito.Application.Usuarios;
using InterfacesComProposito.Application.Usuarios.ConsultarUsuario;
using InterfacesComProposito.Pricing.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace InterfacesComProposito.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
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
