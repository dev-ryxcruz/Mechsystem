# MechSystem 🚀

**MechSystem** é uma aplicação web moderna e abrangente, projetada especificamente para oficinas mecânicas. Desenvolvida com **.NET 10** e **Blazor Server**, ela oferece uma plataforma robusta e responsiva para gerenciar clientes, veículos, serviços e em geral, o fluxo de trabalho operacional de forma eficiente.

## 🌟 Principais Funcionalidades

* **Autenticação e Autorização**: Sistema de login seguro com controle de acesso baseado em perfis (Administrador, etc.), utilizando `BCrypt.Net-Next` e autenticação baseada em Cookies.
* **Dashboard Estratégico (Business Intelligence)**: Painel de alta performance com indicadores operacionais e financeiros cruciais para a oficina, incluindo:
  * Distribuição de Receitas (Mão de obra vs. Peças).
  * Ticket Médio.
  * Funil de Serviços.
  * Análise de participação de mercado por marca de veículo.
  * Rastreamento de idade da frota.
* **Gestão de Ordens de Serviço (OS)**: Acompanhamento completo do ciclo de vida da OS, desde a criação até o encerramento, automatizando processos de negócio.
* **Fluxos de Vistoria Obrigatórios**: Garantia de que as inspeções vitais dos veículos sejam realizadas e devidamente documentadas no sistema.
* **Catálogo de Serviços Integrado**: Catálogo pré-definido no processo de criação da OS para facilitar o cálculo automático de custos de mão de obra e documentação.
* **Cadastros (Repositórios)**: Gerenciamento estruturado de banco de dados de clientes e seus respectivos veículos.
* **Módulo de Configurações do Sistema**: Customização das opções e preferências globais da aplicação para se adequar a necessidades específicas.
* **UI/UX Moderna**: Layout limpo, premium e responsivo (usando CSS Nativo), com navegação dinâmica (sidebar) e transições suaves, utilizando identidade visual de marca focada na cor primária `#ff751f`.

## 🛠️ Tecnologias Utilizadas

* **Framework de Backend/Frontend**: .NET 10, ASP.NET Core Blazor Web App (Interactive Server Mode).
* **Linguagem**: C# 
* **ORM**: Entity Framework Core 10
* **Banco de Dados**: SQLite (nativo da aplicação, arquivo `Mechsystem.db`) e preparado para PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`).
* **Estilização**: CSS Nativo moderno.
* **Segurança**: Autenticação com Cookies e encriptação de senhas com BCrypt.

## ⚙️ Pré-requisitos

Para rodar este projeto, você precisará ter instalado na sua máquina:

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) ou superior.
* Git

## 🚀 Como Rodar o Projeto

Siga os passos abaixo para testar ou desenvolver na aplicação localmente:

1. **Clone o repositório**:
   ```bash
   git clone https://github.com/seu-usuario/MechSystem.git
   cd MechSystem
   ```

2. **Restaure as dependências do projeto**:
   ```bash
   dotnet restore
   ```

3. **Inicie a aplicação (Hot Reload habilitado)**:
   ```bash
   dotnet watch run
   ```
   > **Nota**: O banco de dados (`Mechsystem.db`) via SQLite será criado e migrado automaticamente na primeira vez que a aplicação for executada.

## 🔐 Credenciais Padrão (Admin)

No primeiro acesso, o sistema cria automaticamente um usuário administrador padrão para gerenciar a oficina.

* **Username**: `admin`
* **Senha**: `admin123`

Se em algum momento for necessário redefinir a senha do admin para a padrão, basta rodar a flag:
```bash
dotnet run -- --reset-admin
```

## 📁 Estrutura do Projeto

* `/Components`: Componentes UI do Blazor Server (Páginas, Layouts e componentes compartilhados como a Sidebar).
* `/Data`: Configurações do Entity Framework (`AppDbContext`), banco de dados.
* `/Endpoints`: Minimal APIs para fluxos específicos, como o Login/Logout integrado ao Cookie Scheme.
* `/Interfaces`: Contratos e abstrações para injeção de dependências.
* `/Models`: Entidades de domínio (Usuario, Cliente, Veiculo, Servico, OS, Configuracao, etc.).
* `/Repositories`: Camada de acesso a dados e consultas ao DB.
* `/Services`: Lógica de negócios.
* `/wwwroot`: Assets estáticos, imagens, logos e folhas de estilo CSS globais (`app.css`, etc.).

## 🤝 Como Contribuir

Contribuições são muito bem-vindas! Se você deseja colaborar:
1. Faça um Fork do projeto.
2. Crie uma branch para a sua feature (`git checkout -b feature/NovaFuncionalidade`).
3. Commit suas mudanças (`git commit -m 'Adiciona a NovaFuncionalidade'`).
4. Faça o Push para a branch (`git push origin feature/NovaFuncionalidade`).
5. Abra um Pull Request.

---

## 👨‍💻 Autor

Desenvolvido com dedicação por **Ryan Cristian**.
Este projeto reflete o meu foco em criar soluções de software escaláveis e interfaces modernas.

*   [LinkedIn](https://www.linkedin.com/in/ryan-cristian-a0889324b)
*   [Portfólio (GitHub)](https://github.com/dev-ryxcruz)
