# Estágio 1: Build (.NET 10 SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia arquivo de projeto e restaura dependências
COPY ["mechsystem.csproj", "./"]
RUN dotnet restore "mechsystem.csproj"

# Copia o restante do código-fonte (excluindo arquivos de teste no publish da aplicação principal)
COPY . .
RUN dotnet publish "mechsystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime (.NET 10 ASP.NET Core)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Instala tzdata para garantir resolução do fuso America/Sao_Paulo em containers Linux
USER root
RUN apt-get update && apt-get install -y --no-install-recommends tzdata && rm -rf /var/lib/apt/lists/*
USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "mechsystem.dll"]
