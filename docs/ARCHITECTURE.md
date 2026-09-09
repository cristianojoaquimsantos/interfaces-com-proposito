# Arquitetura da Solução: Uso Consciente de Interfaces e Clean Architecture

## 1. Visão Geral

Este documento detalha as decisões arquiteturais tomadas no projeto `InterfacesComProposito`, servindo como material de apoio conceitual ao artigo:
> **"Você realmente precisa de uma `IAlgumaCoisaService` no seu projeto? Quando uma interface representa uma abstração de verdade — e quando ela existe apenas por convenção."**

---

## 2. Matriz de Decisão: Interface vs. Classe Concreta

A tabela a seguir sumariza a decisão e a justificativa para cada componente central da solução:

| Componente | Interface? | Motivo Arquitetural Concreto |
|---|:---:|---|
| `UsuarioService` | **Não** | Componente interno da mesma camada (`Application`). Não há fronteira tecnológica a isolar, não há dependência externa e não existem múltiplas implementações. Injetado diretamente como classe concreta via DI Container. |
| `ConsultarUsuarioHandler` | **Não** | Caso de uso concreto invocado diretamente pelo Controller HTTP. Não há necessidade de polimorfismo ou inversão de dependência. |
| `CriarPedidoHandler` | **Não** | Caso de uso concreto que orquestra a criação do agregado. Interface geraria duplicação inútil de 1:1. |
| `ConsultarPedidoHandler` | **Não** | Caso de uso concreto de consulta. |
| `AdicionarItemPedidoHandler` | **Não** | Caso de uso concreto de mutação do Aggregate Root. |
| `FinalizarPedidoHandler` | **Não** | Caso de uso concreto de finalização de pedidos. |
| `AnexarComprovantePedidoHandler`| **Não** | Caso de uso concreto para manipulação de uploads. |
| `IRepositorioUsuarios` | **Sim** | **Porta de persistência pertencente à Application**. Aplicação do DIP para que os casos de uso não dependam de EF Core ou SQL Server. |
| `IRepositorioPedidos` | **Sim** | **Porta de persistência pertencente à Application**. Isola a recuperação e armazenamento do Aggregate Root `Pedido`. |
| `IRepositorioProdutos` | **Sim** | **Porta de persistência pertencente à Application**. Isola consultas de catálogo e regras de disponibilidade. |
| `IFileStorage` | **Sim** | **Isola armazenamento externo**. A aplicação lida com Streams e nomes lógicos sem conhecer os SDKs do Azure Blob Storage. |
| `IMessagePublisher` | **Sim** | **Isola mensageria**. A aplicação publica eventos sem conhecer detalhes do Azure Service Bus. |
| `ISecretProvider` | **Sim** | **Isola provider de secrets**. Isola a recuperação segura de credenciais via Azure Key Vault em tempo de execução. |
| `IPagamentoGateway` | **Sim** | **Integração externa**. Isola operações de cobrança externa sujeitas a protocolos de terceiros e adquirentes variáveis. |
| `INotificador` | **Sim** | **Isola canais de notificação externa**. Permite envio de avisos (e-mail, etc.) desacoplado do fluxo transacional. |
| `IAuditoria` | **Sim** | **Porta semântica de auditoria**. Registra eventos de conformidade e auditoria sem vazar detalhes de armazenamento de log. |
| `ICalculadoraDePreco` | **Sim** | **Contrato público compartilhável**. Interface com implementação única que existe para servir como contrato estável para múltiplos times e microsserviços. |

---

## 3. Direção das Dependências (Clean Architecture)

A direção obrigatória do projeto respeita:

```text
       ┌───────────────────────────────┐
       │ InterfacesComProposito.Domain │
       └───────────────┬───────────────┘
                       ▲
                       │ depende de
       ┌───────────────┴───────────────┐
       │ InterfacesComProposito.       │ ◄──── InterfacesComProposito.Pricing.Contracts
       │ Application                   │
       └───────────────┬───────────────┘
                       ▲
                       │ depende de
       ┌───────────────┴───────────────┐
       │ InterfacesComProposito.       │
       │ Infrastructure                │
       └───────────────┬───────────────┘
                       ▲
                       │ depende de
       ┌───────────────┴───────────────┐
       │ InterfacesComProposito.Api    │ (Composition Root)
       └───────────────────────────────┘
```

### O Princípio da Inversão de Dependência em Ação

No caso da finalização de um pedido:

```text
FinalizarPedidoHandler
        │
        ▼
IRepositorioPedidos  ◄──────  RepositorioPedidosSqlServer
(declarada na Application)     (implementada na Infrastructure)
```

As setas representam **"depende de"**. As duas dependências convergem para a abstração pertencente à camada Application. Isso é Dependency Inversion.

Em contrapartida, para comunicação interna na mesma camada:

```text
CriarPedidoHandler  ──────►  UsuarioService
(classe concreta)            (classe concreta)
```

Ambas pertencem à mesma camada (`Application`). Ambas compartilham o mesmo ciclo de vida. Uma interface entre elas não mudaria a direção de nenhuma dependência e não traria nenhuma vantagem arquitetural real.

---

## 4. O Anti-Pattern do Construtor Inflado

O artigo discute o clássico anti-pattern onde arquiteturas supostamente "limpas" acabam gerando construtores como:

```csharp
// ANTI-PATTERN: Falsa sensação de desacoplamento
public ProcessarPedidoHandler(
    IUsuarioService usuarioService,
    IProdutoService produtoService,
    IPagamentoService pagamentoService,
    IEnderecoService enderecoService,
    INotificacaoService notificacaoService,
    IAuditoriaService auditoriaService)
```

Neste projeto, eliminamos essa prática:
1. `Usuario` é validado através da classe concreta interna `UsuarioService`.
2. `Endereco` é um **Value Object** que protege suas próprias invariantes no Domínio, sem necessidade de `IEnderecoService`.
3. `Pedido` é um **Aggregate Root** que calcula seu próprio total e gerencia seus itens, eliminando a necessidade de serviços intermediários artificiais.
4. As dependências injetadas representam **fronteiras de I/O reais**: persistência (`IRepositorioPedidos`), pagamento (`IPagamentoGateway`), mensageria (`IMessagePublisher`), notificação (`INotificador`) e auditoria (`IAuditoria`).
