# MechSystem 🚀

**MechSystem** é uma aplicação web moderna para gestão operacional e financeira de oficinas mecânicas, desenvolvida com **.NET 10**, **Blazor Server** e **PostgreSQL**.

## 🌟 Principais Funcionalidades

* **Autenticação, Autorização e Segurança**:
  * Controle de acesso baseado em três perfis (`Administrador`, `Atendimento` e `Mecanico`), com revalidação automática de sessão (`OnValidatePrincipal`).
  * Senhas com hash `BCrypt.Net-Next`, política mínima de complexidade e proteção contra força bruta (*Rate Limiting*) no endpoint de login.
* **Gestão de Ordens de Serviço (O.S.)**:
  * Máquina de estados centralizada (`Orcamento` → `EmAndamento` ⇄ `Pausada` / `AguardandoPecas` → `Concluida`, além de `Cancelada`) validada no servidor.
  * Checklist obrigatório de vistoria de entrada, catálogo de serviços e vínculo de peças com validação de desconto por perfil.
  * Rastreamento público da O.S. sem necessidade de login (`/acompanhar/{token}`) com **QR Code gerado localmente** via `QRCoder`.
* **Controle de Estoque e Movimentações**:
  * Baixa e estorno de estoque unificados e transacionais vinculados ao ciclo de vida da O.S., prevenindo baixa duplicada ou saldo negativo.
  * Controle de concorrência otimista via token `xmin` do PostgreSQL na entidade `Peca`.
  * Histórico auditável de entradas, saídas, ajustes e estornos (`MovimentacaoEstoque`).
* **Painel do Mecânico**:
  * Visão dedicada para a oficina iniciar, pausar (informando motivo) e finalizar a execução técnica de O.S. aprovadas.
* **Dashboard Estratégico e Financeiro**:
  * Indicadores de faturamento real filtrados por `DataConclusao`, margem de lucro líquido real (descontando custo de aquisição das peças e impostos configuráveis), ticket médio e funil operacional.
* **Cadastros Base**:
  * Gestão de Clientes (CPF/CNPJ único normalizado, exclusão lógica), Veículos (placa única normalizada), Catálogo de Serviços, Peças e Usuários.

## 🛠️ Tecnologias Utilizadas

* **Backend / Frontend**: .NET 10, ASP.NET Core Blazor Web App (Interactive Server Mode)
* **Linguagem**: C#
* **ORM & Banco de Dados**: Entity Framework Core 10 + **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`)
* **Geração de QR Code**: `QRCoder` (geração local em memória, sem envio de URLs a terceiros)
* **Segurança**: Cookie Authentication + `BCrypt.Net-Next` + ASP.NET Core Rate Limiting
* **Testes Automatizados**: xUnit, EF Core InMemory e Testcontainers / PostgreSQL

## ⚙️ Pré-requisitos

* **Com Docker**: [Docker](https://www.docker.com/) e Docker Compose.
* **Sem Docker (Execução Local)**:
  * [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
  * [PostgreSQL 16+](https://www.postgresql.org/)

## 🚀 Como Rodar o Projeto

### Opção 1: Com Docker Compose (Recomendado)

1. **Suba o banco PostgreSQL e a aplicação**:
   ```bash
   docker compose up -d --build
   ```
2. Acesse em `http://localhost:8080`.
3. As migrações do EF Core são aplicadas automaticamente na inicialização.

### Opção 2: Execução Local (Sem Docker)

1. **Configure a string de conexão PostgreSQL** em `appsettings.Development.json`, User Secrets ou variável de ambiente:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=mechsystem;Username=postgres;Password=sua_senha"
   ```
2. **Restaure os pacotes e inicie a aplicação**:
   ```bash
   dotnet restore
   dotnet run
   ```

## 🔐 Configuração da Senha Inicial do Administrador

O sistema **não utiliza senhas fixas hardcoded** no código-fonte. Na primeira inicialização (quando o usuário `admin` ainda não existe ou possui hash legado):

1. Você pode definir a senha inicial via configuração (`Admin:InitialPassword`) ou variável de ambiente (`Admin__InitialPassword`):
   ```bash
   dotnet user-secrets set "Admin:InitialPassword" "SuaSenhaForte@2026"
   ```
2. Caso `Admin:InitialPassword` não seja informada, o sistema **gera automaticamente uma senha aleatória criptograficamente segura de 16 caracteres** e a exibe **uma única vez** no log de inicialização (`ILogger`).

Para redefinir a senha do usuário `admin` a qualquer momento:
```bash
dotnet run -- --reset-admin
```

## 🧪 Populando Dados de Demonstração (`--seed-demo`)

O banco não é populado automaticamente com dados fictícios em produção. Para carregar o conjunto realista e idempotente de demonstração (clientes, veículos, peças, movimentações e O.S. em múltiplos estados):

* **Localmente**:
  ```bash
  dotnet run -- --seed-demo
  ```
* **Com Docker Compose**:
  ```bash
  docker compose run --rm app --seed-demo
  ```

## ✅ Executando os Testes Automatizados

O repositório possui a suíte `mechsystem.Tests` cobrindo transições de estado da O.S., regras de estoque, bloqueio de desconto por perfil, cálculo de margem/totais e atomicidade transacional no PostgreSQL:

```bash
dotnet test --verbosity normal
```

## 🔒 Privacidade e Conformidade com a LGPD (Lei nº 13.709/2018)

O MechSystem adota princípios de minimização, finalidade e segurança no tratamento de dados pessoais de clientes e colaboradores:

* **Dados Pessoais Armazenados**: Nome completo, CPF, telefone, e-mail e endereço do cliente (`Cliente`), além da placa do veículo (`Veiculo`) e histórico de contatos na O.S. (`ContatoOS`).
* **Finalidade e Base Legal**: Execução de contrato de prestação de serviços mecânicos e emissão de orçamento prévio obrigatório nos termos do **Art. 40 do Código de Defesa do Consumidor (CDC)**, garantia legal de peças/serviços e comunicação operacional sobre o andamento do reparo.
* **Medidas Técnicas de Proteção**:
  * **Normalização e Integridade**: CPF e telefone são sanitizados (apenas dígitos) antes da persistência; senhas de operadores são armazenadas exclusivamente com hash `BCrypt`.
  * **Exclusão Lógica (*Soft Delete*)**: Clientes inativados (`Ativo = false`) permanecem ocultos das listagens operacionais via *Global Query Filter*, preservando apenas a integridade histórica fiscal/contratual sem reativação indevida.
  * **Privacidade no Acompanhamento Público**: O link público (`/acompanhar/{token}`) utiliza token opaco aleatório de 128 bits (`Guid` hexadecimal) e **QR Code renderizado localmente no servidor (`QRCoder`)**, sem enviar URLs ou dados da O.S. para APIs externas. No portal público, dados sensíveis do proprietário (CPF, endereço, telefone) não são exibidos.
  * **Segurança de Sessão e Acesso**: Cookies `HttpOnly`, `SameSite=Strict`, `SecurePolicy=Always` em produção, revalidação automática de vínculo/perfil (`OnValidatePrincipal`), controle de acesso por papéis no servidor e *Rate Limiting* contra ataques de força bruta no login.

## 📁 Estrutura do Projeto

* `/Components`: Componentes e páginas Blazor Server (`Pages/OrdensServico`, `Pages/PainelMecanico`, `Pages/Acompanhar`, `Pages/Estoque`, etc.).
* `/Data`: `AppDbContext` (com conversão automática de `DateTime` para UTC) e `DatabaseSeeder`.
* `/Endpoints`: Minimal APIs de autenticação (`/api/auth/login` com rate limiting e `/api/auth/logout`).
* `/Models`: Entidades de domínio (`OrdemServico`, `Peca` com token `xmin`, `MovimentacaoEstoque`, `Cliente`, `Veiculo`, `Usuario`, etc.).
* `/Repositories`: Repositórios de acesso a dados.
* `/Services`: Camada de regras de negócio (`OrdemServicoService`, `EstoqueService`, `UsuarioService`, `DomainErrorHelper`, `DateTimeExtensions`).
* `/mechsystem.Tests`: Projeto xUnit de testes unitários e de integração.
* `/Documentacao`: Documentação técnica de arquitetura, regras de negócio, LGPD e relatório de estabilização.

---

## 👨‍💻 Autor

Desenvolvido por **Ryan Cristian**.

* [LinkedIn](https://www.linkedin.com/in/ryan-cristian-a0889324b)
* [Portfólio (GitHub)](https://github.com/dev-ryxcruz)
