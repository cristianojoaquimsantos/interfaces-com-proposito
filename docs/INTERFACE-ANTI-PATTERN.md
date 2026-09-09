# O Anti-Pattern da Duplicação 1:1 de Interfaces

> **Aviso**: Os exemplos de código a seguir representam um **anti-pattern comum** em projetos .NET e são apresentados aqui **exclusivamente como documentação de apoio conceitual ao artigo**, não tendo sido implementados no código de produção desta solução.

---

## 1. O Padrão a Ser Evitado

Em muitas soluções empresariais, convencionou-se que qualquer classe com o sufixo `Service` ou que utilize Injeção de Dependências deve obrigatoriamente possuir uma interface idêntica com o prefixo `I`:

```text
Application
├── Interfaces/
│   ├── IUsuarioService.cs
│   ├── IPedidoService.cs
│   └── IProdutoService.cs
└── Services/
    ├── UsuarioService.cs
    ├── PedidoService.cs
    └── ProdutoService.cs
```

O código resultante costuma ser um espelhamento estéril:

```csharp
// ANTI-PATTERN: Espelhamento 1:1 sem propósito
public interface IUsuarioService
{
    Task<UsuarioDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CriarUsuarioAsync(CriarUsuarioDto dto, CancellationToken cancellationToken);
}

public sealed class UsuarioService : IUsuarioService
{
    // Implementação única, idêntica e sem qualquer perspectiva de polimorfismo
}
```

E no `Program.cs`:

```csharp
services.AddScoped<IUsuarioService, UsuarioService>();
services.AddScoped<IPedidoService, PedidoService>();
services.AddScoped<IProdutoService, ProdutoService>();
```

---

## 2. Por que isso NÃO significa Boas Práticas?

### 2.1 Não significa Baixo Acoplamento
Se toda vez que você altera a assinatura de um método na classe concreta precisa alterar também a interface correspondente (porque só existe aquela classe no universo do sistema), a interface **não está isolando nada**.  
O acoplamento conceitual entre o chamador e a implementação continua sendo de 100%. A interface passa a ser apenas um "imposto de manutenção" que duplica o trabalho a cada refatoração.

### 2.2 Não significa Aplicação do Dependency Inversion Principle (DIP)
O DIP estabelece que:
> *"Módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações."*

Quando você cria `IUsuarioService` dentro da camada `Application` e a implementa em `UsuarioService` na **mesma** camada `Application`, nenhuma dependência entre camadas foi invertida. O fluxo de dependências continua exatamente o mesmo.  
A inversão de dependência faz sentido onde existe uma **fronteira arquitetural** (por exemplo: Application define `IRepositorioPedidos` e Infrastructure implementa `RepositorioPedidosSqlServer`).

### 2.3 Não significa Melhor Testabilidade
Um dos argumentos mais frequentes para criar interfaces 1:1 é: *"precisamos da interface para mockar no teste unitário"*.

Esse argumento falha por duas razões:
1. **Mockar detalhes internos produz testes frágeis**: Se você testa um caso de uso mockando serviços internos da mesma camada com `mock.Verify(x => x.AlgumMetodo(), Times.Once)`, qualquer refatoração interna que não altere o resultado final quebra o teste.
2. **O teste deve focar em comportamento observável e estado**: É muito mais robusto e simples testar classes concretas diretamente ou utilizar fakes leves em memória para portas de I/O, em vez de contaminar o design de produção com interfaces criadas apenas para satisfazer bibliotecas de mock.

### 2.4 Não significa Clean Architecture
Clean Architecture trata de **limites arquiteturais**, **regras de negócio isoladas de mecanismos de entrega** e **direção correta de dependências**.  
Colocar pastas `Interfaces` e `Services` e prefixar todas as classes com `I` é apenas mimetismo sintático. Clean Architecture incentiva modelar por casos de uso e proteger o domínio com agregados e entidades ricas, em vez de criar serviços anêmicos cheios de métodos CRUD.

---

## 3. O Critério Decisório de Arquitetura

Antes de criar qualquer interface, faça o seguinte checklist:

```text
1. Quem consome essa abstração?
2. Quem a implementa?
3. Quem é o dono do contrato?
4. Existe uma fronteira arquitetural ou tecnológica a ser isolada?
5. A interface muda a direção da dependência entre camadas?
6. Existe um contrato público estável com múltiplos consumidores externos?
7. Estou criando isso apenas para mockar? (Se sim, pare).
8. Estou criando isso apenas por convenção ou sufixo? (Se sim, pare).
```

Se a interface não passar com clareza pelos itens de 1 a 6, **prefira a classe concreta**.  
A simplicidade é a maior virtude de uma arquitetura sustentável.
