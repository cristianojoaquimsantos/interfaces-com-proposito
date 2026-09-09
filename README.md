# InterfacesComProposito

> **Você realmente precisa de uma `IAlgumaCoisaService` no seu projeto?**  
> *Quando uma interface representa uma abstração de verdade — e quando ela existe apenas por convenção.*

Projeto de referência em **.NET 10 / C#** e **ASP.NET Core Web API** desenvolvido para apoiar uma discussão técnica e arquitetural aprofundada sobre o uso consciente de interfaces, Inversão de Dependência (DIP - *Dependency Inversion Principle*) e Injeção de Dependências (DI - *Dependency Injection*).

---

## 1. Princípio Central

Antes de criar qualquer interface, faça a seguinte pergunta:

> **Qual problema arquitetural essa interface está resolvendo?**

Uma interface somente deve existir quando houver uma justificativa concreta:
- **Inversão da direção de dependência (DIP)** entre Application e Infrastructure;
- **Isolamento de uma tecnologia externa** (banco de dados, nuvem, mensageria, storage);
- **Definição de uma porta de aplicação** (Hexagonal / Clean Architecture);
- **Existência de comportamentos ou estratégias genuinamente intercambiáveis**;
- **Contrato público estável** consumido por componentes externos / múltiplos times;
- **Fronteira relevante** que precisa permanecer estável independentemente da tecnologia.

### O que NÃO justifica criar uma interface:
- A classe utiliza Dependency Injection (DI resolve classes concretas nativamente);
- "Para facilitar mocking" (não desenhe a arquitetura em torno de detalhes de ferramentas de teste);
- A classe termina com o sufixo `Service`;
- "Existe uma convenção `IClasse -> Classe` na equipe";
- "Talvez uma segunda implementação apareça algum dia" (princípio YAGNI);
- Geradores automáticos que espelham 1:1 todas as classes concretas da solução.

---

## 2. Por que algumas classes têm interface e outras não?

Este é o ponto central demonstrado neste repositório.

### 2.1 Por que NÃO existe:

| Classe Concreta | Camada | Por que NÃO tem interface? |
|---|---|---|
| `UsuarioService` | `Application` | Representa lógica de negócio interna da própria camada Application. Não há fronteira tecnológica a isolar, não há dependência externa e não existem múltiplas implementações concorrentes. É registrada diretamente no DI Container via `services.AddScoped<UsuarioService>()` e injetada diretamente nos handlers. Criar `IUsuarioService` geraria apenas duplicação estrutural sem nenhum ganho arquitetural. |
| `ConsultarUsuarioHandler` | `Application` | Caso de uso concreto invocado diretamente pelo controller HTTP. Criar `IConsultarUsuarioHandler` seria espelhar cada caso de uso em uma interface estéril de método único sem nenhum polimorfismo. |
| `CriarPedidoHandler` | `Application` | Caso de uso concreto de criação de pedidos. É chamado diretamente pela camada web (Api) e orquestra o Aggregate Root e os serviços da própria camada. |
| `FinalizarPedidoHandler` | `Application` | Caso de uso concreto de finalização de pedidos. Não possui implementações alternativas; orquestra as portas reais de infraestrutura necessárias. |
| `ConsultarPedidoHandler` | `Application` | Caso de uso concreto de leitura. |
| `AdicionarItemPedidoHandler`| `Application` | Caso de uso concreto de modificação do agregado. |
| `AnexarComprovantePedidoHandler`| `Application`| Caso de uso concreto de anexo de arquivo. |

---

### 2.2 Por que EXISTEM:

#### Interface: `IRepositorioUsuarios`
* **Consumidor:** `Application` (`UsuarioService`, `ConsultarUsuarioHandler`)
* **Implementação:** `RepositorioUsuariosSqlServer` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Persistence`)
* **Fronteira protegida:** Mecanismo de persistência e banco de dados relacional.
* **Motivo da abstração:** Aplicação do Princípio da Inversão de Dependência (DIP). A regra de negócio da aplicação não pode depender diretamente do Entity Framework Core ou do SQL Server. A Application declara a porta e a Infrastructure a implementa.

---

#### Interface: `IRepositorioPedidos`
* **Consumidor:** `Application` (`CriarPedidoHandler`, `FinalizarPedidoHandler`, etc.)
* **Implementação:** `RepositorioPedidosSqlServer` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Persistence`)
* **Fronteira protegida:** Ciclo de vida e persistência do Aggregate Root `Pedido`.
* **Motivo da abstração:** Permite que a camada de aplicação carregue e persista agregados completos atomicamente sem conhecer mapeamentos relacionais, queries SQL ou tracking de ORM.

---

#### Interface: `IRepositorioProdutos`
* **Consumidor:** `Application` (`CriarPedidoHandler`, `AdicionarItemPedidoHandler`)
* **Implementação:** `RepositorioProdutosSqlServer` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Persistence`)
* **Fronteira protegida:** Catálogo de produtos.
* **Motivo da abstração:** Isola consultas de catálogo e regras de disponibilidade de itens sem vazar consultas ou DbContext para os casos de uso.

---

#### Interface: `IFileStorage`
* **Consumidor:** `Application` (`AnexarComprovantePedidoHandler`)
* **Implementação:** `AzureBlobFileStorage` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Storage`)
* **Fronteira protegida:** Armazenamento externo de arquivos / objetos binários.
* **Motivo da abstração:** Impedir que os casos de uso dependam dos SDKs do Azure Blob Storage (`BlobServiceClient`, `BlobContainerClient`). A aplicação trabalha com `Stream` e strings; a infraestrutura traduz para a nuvem da Microsoft ou armazenamento local.

---

#### Interface: `IMessagePublisher`
* **Consumidor:** `Application` (`FinalizarPedidoHandler`)
* **Implementação:** `AzureServiceBusPublisher` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Messaging`)
* **Fronteira protegida:** Infraestrutura de mensageria e comunicação assíncrona entre sistemas.
* **Motivo da abstração:** A aplicação publica eventos sem saber se o broker é Azure Service Bus, RabbitMQ, Kafka ou um log em memória. A abstração representa a **capacidade** ("publicar mensagem") e não o produto.

---

#### Interface: `ISecretProvider`
* **Consumidor:** `Infrastructure` / `Application`
* **Implementação:** `AzureKeyVaultSecretProvider` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Security`)
* **Fronteira protegida:** Cofre de segredos e credenciais de segurança.
* **Motivo da abstração:** Isola a recuperação de secrets em tempo de execução via `DefaultAzureCredential`, protegendo o sistema contra hardcoding de chaves e acoplamento direto à API do Azure Key Vault.

---

#### Interface: `IPagamentoGateway`
* **Consumidor:** `Application` (`FinalizarPedidoHandler`)
* **Implementação:** `PagamentoGatewaySimulado` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Payments`)
* **Fronteira protegida:** Gateway financeiro de terceiros / adquirente de pagamentos.
* **Motivo da abstração:** Isola operações de cobrança externa sujeitas a contratos de rede, adquirentes variáveis (Cielo, Stripe, etc.) e falhas externas de processamento financeiro.

---

#### Interface: `INotificador`
* **Consumidor:** `Application` (`FinalizarPedidoHandler`)
* **Implementação:** `NotificadorEmail` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Notifications`)
* **Fronteira protegida:** Canais externos de comunicação (E-mail, Teams, SMS).
* **Motivo da abstração:** Desacopla o fluxo central do pedido dos protocolos de envio de mensagens externas.

---

#### Interface: `IAuditoria`
* **Consumidor:** `Application` (`FinalizarPedidoHandler`)
* **Implementação:** `AuditoriaLog` (em `Infrastructure`)
* **Camada que possui o contrato:** `Application` (`Application/Ports/Audit`)
* **Fronteira protegida:** Trilha de auditoria externa e conformidade regulatória.
* **Motivo da abstração:** Porta semântica com foco na capacidade de negócio, rejeitando a convenção rasa de criar "IAuditoriaService".

---

#### Interface: `ICalculadoraDePreco`
* **Consumidor:** `Application` (`CriarPedidoHandler`) / Consumidores externos
* **Implementação:** `CalculadoraDePrecoPadrao` (em `Application/Pricing`)
* **Camada que possui o contrato:** `InterfacesComProposito.Pricing.Contracts` (Biblioteca compartilhável)
* **Fronteira protegida:** API pública compartilhada entre sistemas/equipes.
* **Motivo da abstração:** **Demonstração intencional do caso de contrato público com implementação única**. A interface não existe por causa de múltiplas implementações locais, mas sim porque representa um contrato público consumido por múltiplos serviços com ciclo de vida independente da implementação.

---

## 3. Arquitetura e Fluxo de Dependências

```text
Domain
  ↑ (depende de)
Application (define Ports / Interfaces necessárias)
  ↑ (depende de)
Infrastructure (implementa Adapters concretos)
  ↑ (depende de)
Api (Composition Root)
```

### O contraste no Container de Injeção de Dependências (Composition Root)

Ao abrir a configuração de DI na solução, a distinção é evidente:

```csharp
// Application: componentes internos registrados diretamente como classes concretas
services.AddScoped<UsuarioService>();
services.AddScoped<ConsultarUsuarioHandler>();
services.AddScoped<CriarPedidoHandler>();
services.AddScoped<FinalizarPedidoHandler>();

// Infrastructure: fronteiras arquiteturais registradas como abstração + adapter
services.AddScoped<IRepositorioUsuarios, RepositorioUsuariosSqlServer>();
services.AddScoped<IRepositorioPedidos, RepositorioPedidosSqlServer>();
services.AddScoped<IFileStorage, AzureBlobFileStorage>();
services.AddScoped<IMessagePublisher, AzureServiceBusPublisher>();
services.AddScoped<IPagamentoGateway, PagamentoGatewaySimulado>();
```

---

## 4. Como Executar a Aplicação

### Pré-requisitos
- .NET SDK 10 instalado.

### Executar a API
```bash
dotnet restore
dotnet build
dotnet run --project src/InterfacesComProposito.Api/InterfacesComProposito.Api.csproj
```

A API inicia automaticamente com um banco inicializado em memória/LocalDB e dados de exemplo (usuário padrão e catálogo com teclado, mouse e monitor).

### Endpoints Principais
- `GET /api/usuarios/{id}` - Consulta usuário por Id.
- `POST /api/pedidos` - Criação de pedido com itens do catálogo.
- `GET /api/pedidos/{id}` - Consulta de pedido.
- `POST /api/pedidos/{id}/itens` - Adição de item e recálculo automático.
- `POST /api/pedidos/{id}/finalizar` - Finalização, pagamento simulado e evento no broker.
- `POST /api/pedidos/{id}/comprovante` - Upload de anexo multipart/form-data.

---

## 5. Como Executar os Testes

```bash
dotnet test InterfacesComProposito.sln
```

A suíte cobre:
- **`Domain.Tests`**: Invariantes de negócio de Pedido, Usuário, ItemPedido e Endereço.
- **`Application.Tests`**: Testes de comportamento do `UsuarioService` (classe concreta direta) e dos handlers com stubs/fakes.
- **`Api.IntegrationTests`**: Testes de ponta a ponta via `WebApplicationFactory` cobrindo o fluxo completo e respostas ProblemDetails (400, 404, 409).

---

## 6. Documentos Complementares
- [Matriz de Decisão Arquitetural](docs/ARCHITECTURE.md)
- [Análise do Anti-Pattern 1:1 de Interfaces](docs/INTERFACE-ANTI-PATTERN.md)
