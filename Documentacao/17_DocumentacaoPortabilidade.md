# Documentação de Portabilidade — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Documentar as capacidades de **portabilidade** do sistema MechSystem, descrevendo como a aplicação pode ser implantada em diferentes sistemas operacionais, bancos de dados, ambientes de execução e plataformas de nuvem.

---

## 2. Portabilidade de Sistema Operacional

### 2.1 Plataformas Suportadas

| Sistema Operacional | Versão Mínima | Suporte | Testado |
|--------------------|---------------|---------|---------|
| **Windows** | 10 / Server 2016+ | ✅ Nativo | ✅ Sim |
| **Linux** | Ubuntu 20.04+, Debian 11+, CentOS 8+ | ✅ Nativo | ✅ Sim |
| **macOS** | 12 Monterey+ | ✅ Nativo | ⬜ Não |

### 2.2 Justificativa Técnica

O MechSystem é construído com **.NET 10**, que possui **runtime multiplataforma** nativo. O .NET compila para **código intermediário (IL)** que é executado pelo **CLR (Common Language Runtime)**, disponível para Windows, Linux e macOS.

### 2.3 Pré-requisitos por Plataforma

#### Windows
```
# Instalar .NET 10 Runtime
winget install Microsoft.DotNet.Runtime.10
# ou SDK para desenvolvimento
winget install Microsoft.DotNet.SDK.10
```

#### Linux (Ubuntu/Debian)
```bash
# Adicionar repositório Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-10.0
```

#### macOS
```bash
# Via Homebrew
brew install dotnet@10
```

### 2.4 Publicação Self-Contained (sem dependência de runtime)

O MechSystem pode ser publicado como **aplicação self-contained**, incluindo o runtime .NET no pacote. Isso elimina a necessidade de instalar o runtime na máquina de destino.

```bash
# Publicar para Windows x64
dotnet publish -c Release -r win-x64 --self-contained true

# Publicar para Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true

# Publicar para macOS ARM (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained true
```

| RID (Runtime Identifier) | Plataforma | Arquitetura |
|--------------------------|-----------|-------------|
| `win-x64` | Windows | 64-bit |
| `win-arm64` | Windows | ARM 64-bit |
| `linux-x64` | Linux | 64-bit |
| `linux-arm64` | Linux | ARM 64-bit (Raspberry Pi) |
| `osx-x64` | macOS | Intel |
| `osx-arm64` | macOS | Apple Silicon |

---

## 3. Portabilidade de Banco de Dados

### 3.1 Bancos Suportados

| Banco de Dados | Provider EF Core | Status | Uso Recomendado |
|---------------|-----------------|--------|----------------|
| **SQLite** | `Microsoft.EntityFrameworkCore.Sqlite` | ✅ Padrão atual | Oficinas pequenas, instalação local |
| **PostgreSQL** | `Npgsql.EntityFrameworkCore.PostgreSQL` | ✅ Preparado (NuGet instalado) | Oficinas médias/grandes, multi-usuário |
| **SQL Server** | `Microsoft.EntityFrameworkCore.SqlServer` | ⬜ Compatível (requer configuração) | Ambientes corporativos com licença Microsoft |

### 3.2 Configuração Atual (SQLite)

Arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Mechsystem.db"
  }
}
```

### 3.3 Migração para PostgreSQL

#### Passo 1 — Alterar Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mechsystem;Username=postgres;Password=senha123"
  }
}
```

#### Passo 2 — Alterar Provider no Program.cs
```csharp
// DE (SQLite):
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// PARA (PostgreSQL):
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

#### Passo 3 — Gerar Migrations
```bash
dotnet ef migrations add MigracaoPostgreSQL
dotnet ef database update
```

### 3.4 Considerações de Migração

| Aspecto | SQLite | PostgreSQL |
|---------|--------|-----------|
| Instalação | Zero (arquivo local) | Requer servidor |
| Concorrência | Limitada (1 writer) | Alta (MVCC) |
| Tamanho máximo | ~280 TB (teórico) | Ilimitado (prático) |
| Performance | Boa até ~50k registros | Excelente em qualquer escala |
| Backup | Copiar arquivo .db | pg_dump / pg_restore |
| Custo | Gratuito | Gratuito (open source) |

---

## 4. Portabilidade via Containerização (Docker)

### 4.1 Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["mechsystem.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Criar volume para o banco SQLite
VOLUME /app/data
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/Mechsystem.db"

EXPOSE 8080
ENTRYPOINT ["dotnet", "mechsystem.dll"]
```

### 4.2 Docker Compose

```yaml
version: '3.8'
services:
  mechsystem:
    build: .
    ports:
      - "8080:8080"
    volumes:
      - mechsystem-data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    restart: unless-stopped

  # Opcional: PostgreSQL em produção
  # postgres:
  #   image: postgres:16-alpine
  #   environment:
  #     POSTGRES_DB: mechsystem
  #     POSTGRES_USER: mechsystem
  #     POSTGRES_PASSWORD: ${DB_PASSWORD}
  #   volumes:
  #     - postgres-data:/var/lib/postgresql/data
  #   ports:
  #     - "5432:5432"

volumes:
  mechsystem-data:
  # postgres-data:
```

### 4.3 Comandos Docker

```bash
# Build da imagem
docker build -t mechsystem:1.0 .

# Executar container
docker run -d -p 8080:8080 --name mechsystem mechsystem:1.0

# Com volume persistente
docker run -d -p 8080:8080 -v mechsystem-data:/app/data mechsystem:1.0
```

---

## 5. Portabilidade para Nuvem

### 5.1 Opções de Deploy

| Provedor | Serviço | Custo Estimado | Complexidade |
|---------|---------|---------------|-------------|
| **Azure** | App Service (Linux) | ~$15-50/mês | Baixa |
| **AWS** | Elastic Beanstalk | ~$15-50/mês | Média |
| **Google Cloud** | Cloud Run | ~$10-30/mês | Baixa |
| **DigitalOcean** | App Platform / Droplet | ~$6-24/mês | Baixa |
| **Railway** | Container Deploy | ~$5-20/mês | Muito Baixa |
| **VPS Genérico** | Qualquer (Contabo, Hetzner) | ~$4-15/mês | Média |

### 5.2 Deploy no Azure App Service (Exemplo)

```bash
# Login no Azure
az login

# Criar Resource Group
az group create --name MechSystem-RG --location brazilsouth

# Criar App Service Plan (Linux)
az appservice plan create --name MechSystem-Plan \
  --resource-group MechSystem-RG \
  --sku B1 --is-linux

# Criar Web App
az webapp create --resource-group MechSystem-RG \
  --plan MechSystem-Plan \
  --name mechsystem-app \
  --runtime "DOTNET|10.0"

# Deploy via ZIP
dotnet publish -c Release -o ./publish
cd publish && zip -r ../deploy.zip .
az webapp deploy --resource-group MechSystem-RG \
  --name mechsystem-app \
  --src-path ../deploy.zip
```

---

## 6. Portabilidade de Browser

### 6.1 Navegadores Suportados

| Navegador | Versão Mínima | Suporte | Justificativa |
|-----------|--------------|---------|--------------|
| Google Chrome | 90+ | ✅ Total | WebSocket + CSS moderno |
| Microsoft Edge | 90+ | ✅ Total | Baseado em Chromium |
| Mozilla Firefox | 90+ | ✅ Total | WebSocket + CSS moderno |
| Safari | 15+ | ⚠ Parcial | Limitações de WebSocket |
| Opera | 76+ | ✅ Total | Baseado em Chromium |

### 6.2 Requisito Técnico
O Blazor Server utiliza **SignalR (WebSocket)** para comunicação em tempo real entre o servidor e o navegador. Todos os navegadores modernos suportam WebSocket.

---

## 7. Matriz de Portabilidade (Resumo)

| Dimensão | Origem | Destino | Esforço | Risco |
|----------|--------|---------|---------|-------|
| SO | Windows | Linux | Baixo (0h) | Baixo |
| SO | Windows | macOS | Baixo (0h) | Baixo |
| Banco | SQLite | PostgreSQL | Baixo (2h) | Baixo |
| Banco | SQLite | SQL Server | Médio (4h) | Médio |
| Deploy | Local | Docker | Baixo (1h) | Baixo |
| Deploy | Local | Azure | Baixo (2h) | Baixo |
| Deploy | Local | AWS | Médio (4h) | Médio |

---

## 8. Conclusão

O MechSystem foi projetado desde o início com **portabilidade como requisito não funcional prioritário**. A combinação de .NET 10 (multiplataforma), EF Core (abstração de banco), Docker (containerização) e CSS nativo (sem dependência de build tools) garante que o sistema pode ser implantado em virtualmente qualquer ambiente moderno com esforço mínimo.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
