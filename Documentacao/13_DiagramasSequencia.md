# Diagramas de Sequência — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Objetivo

Apresentar **3 Diagramas de Sequência** que modelam a interação entre objetos (atores, componentes, serviços e banco de dados) nos cenários mais importantes do MechSystem.

---

## 2. Diagrama de Sequência 1 — Processo de Login / Autenticação

### Descrição
Modela a interação entre o Usuário, a página de Login (Blazor), o AuthEndpoints (Minimal API), o AuthService e o Banco de Dados durante o processo de autenticação.

### Referência no Código
- [AuthEndpoints.cs](file:///c:/projeto/mechsystem/Endpoints/AuthEndpoints.cs)
- [AuthService.cs](file:///c:/projeto/mechsystem/Services/AuthService.cs)
- [Login.cs](file:///c:/projeto/mechsystem/Models/Login.cs)

### Diagrama

```mermaid
sequenceDiagram
    actor Usuario as 👤 Usuário
    participant LP as Login.razor<br/>(Blazor Page)
    participant AE as AuthEndpoints<br/>(Minimal API)
    participant AS as AuthService
    participant DB as AppDbContext<br/>(SQLite)
    participant CK as Cookie<br/>Authentication

    Usuario->>LP: Acessa /login
    LP->>Usuario: Exibe formulário<br/>(Username, Senha)
    
    Usuario->>LP: Preenche credenciais<br/>e clica "Entrar"
    LP->>LP: Valida campos<br/>obrigatórios
    
    alt Campos inválidos
        LP-->>Usuario: Exibe mensagens<br/>de validação
    end
    
    LP->>AE: POST /api/auth/login<br/>{Username, Password}
    
    AE->>AS: ValidarLogin(username, password)
    AS->>DB: SELECT * FROM Usuarios<br/>WHERE Username = @username
    DB-->>AS: Usuario encontrado<br/>(ou null)
    
    alt Usuário não encontrado
        AS-->>AE: return null
        AE-->>LP: 401 Unauthorized
        LP-->>Usuario: "Credenciais inválidas"
    end
    
    AS->>AS: BCrypt.Verify(password,<br/>usuario.PasswordHash)
    
    alt Senha incorreta
        AS-->>AE: return null
        AE-->>LP: 401 Unauthorized
        LP-->>Usuario: "Credenciais inválidas"
    end
    
    alt Usuário inativo
        AS-->>AE: return null
        AE-->>LP: 401 Unauthorized
        LP-->>Usuario: "Usuário desativado"
    end
    
    AS-->>AE: return Usuario
    
    AE->>CK: Criar ClaimsPrincipal<br/>(Id, Username,<br/>NomeCompleto, Perfil)
    CK->>CK: HttpContext.SignInAsync()<br/>Cookie: 8h, Sliding, HttpOnly
    CK-->>AE: Cookie criado
    
    AE-->>LP: 200 OK + Set-Cookie
    LP-->>Usuario: Redirect para /<br/>(Dashboard)
    
    Note over CK: Cookie configurado:<br/>SameSite=Strict<br/>SecurePolicy=SameAsRequest<br/>ExpireTimeSpan=8h
```

---

## 3. Diagrama de Sequência 2 — Criação de Ordem de Serviço com Vínculo de Peças

### Descrição
Modela a interação completa durante a criação de uma OS, incluindo a seleção de veículo, vistoria opcional, vínculo de peças e cálculo automático de valores.

### Referência no Código
- [OrdemServico.cs](file:///c:/projeto/mechsystem/Models/OrdemServico.cs)
- [OrdemServicoPeca.cs](file:///c:/projeto/mechsystem/Models/OrdemServicoPeca.cs)
- [EstoqueService.cs](file:///c:/projeto/mechsystem/Services/EstoqueService.cs)

### Diagrama

```mermaid
sequenceDiagram
    actor Atendente as 👤 Atendente
    participant PG as CriarOS.razor<br/>(Blazor Page)
    participant DB as AppDbContext
    participant ES as EstoqueService
    participant OS as OrdemServico<br/>(Model)

    Atendente->>PG: Acessa "Nova OS"
    PG->>DB: Carregar lista de<br/>Veículos com Clientes
    DB-->>PG: List de Veiculos

    Atendente->>PG: Seleciona Veículo<br/>(por placa)
    PG->>PG: Preenche dados do<br/>Cliente automaticamente

    Atendente->>PG: Preenche diagnóstico<br/>e previsão de entrega

    opt Vistoria Obrigatória (Configuracao.ObrigarVistoriaParaOS)
        PG->>DB: Carregar Configuracao
        DB-->>PG: ObrigarVistoriaParaOS = true
        
        Atendente->>PG: Preenche vistoria:<br/>combustível, KM,<br/>checklist, avarias
        PG->>OS: Vincular Vistoria<br/>(relação 1:1)
    end

    Atendente->>PG: Seleciona serviços<br/>do catálogo
    PG->>DB: Carregar lista de Servicos
    DB-->>PG: List de Servicos
    PG->>PG: Calcular ValorMaoDeObra<br/>(soma dos serviços)

    opt Vincular Peças
        Atendente->>PG: Busca peça por<br/>SKU ou nome
        PG->>DB: SELECT * FROM Pecas<br/>WHERE Ativo = true
        DB-->>PG: List de Pecas

        Atendente->>PG: Seleciona peça e<br/>informa quantidade

        PG->>ES: VerificarDisponibilidade<br/>(pecaId, quantidade)
        ES->>DB: SELECT EstoqueAtual<br/>FROM Pecas WHERE Id = @id
        DB-->>ES: EstoqueAtual

        alt Estoque insuficiente
            ES-->>PG: Alerta: estoque<br/>insuficiente
            PG-->>Atendente: Exibe aviso
        end

        PG->>PG: Criar OrdemServicoPeca:<br/>PrecoBase = Peca.PrecoVenda<br/>PrecoCustoSnapshot = Peca.PrecoCusto<br/>ValorCobrado = Peca.PrecoVenda

        Atendente->>PG: Ajusta ValorCobrado<br/>(opcional)

        alt ValorCobrado < PrecoBase
            PG->>PG: Verificar perfil<br/>do usuário
            alt Perfil = Atendimento
                PG-->>Atendente: BLOQUEADO:<br/>"Desconto requer<br/>perfil Administrador"
            end
        end

        PG->>PG: Calcular Subtotal =<br/>Quantidade × ValorCobrado
    end

    PG->>OS: Calcular ValorTotal =<br/>ValorMaoDeObra +<br/>ValorPecasEfetivo

    Atendente->>PG: Clica "Salvar OS"

    PG->>PG: Validar campos<br/>obrigatórios

    PG->>DB: BEGIN TRANSACTION
    PG->>DB: INSERT OrdemServico<br/>(Status: Orçamento)
    PG->>DB: INSERT OrdemServicoPeca<br/>(para cada peça)
    
    PG->>ES: RegistrarSaida(pecaId,<br/>quantidade, "OS #XX")
    ES->>DB: UPDATE Pecas SET<br/>EstoqueAtual -= quantidade
    ES->>DB: INSERT MovimentacaoEstoque<br/>(Tipo: Saída)
    
    PG->>DB: COMMIT

    PG->>PG: Gerar Token de<br/>Acompanhamento

    PG-->>Atendente: Exibe confirmação:<br/>"OS #XX criada"

    Note over OS: ValorTotal = ValorMaoDeObra + <br/>Σ(Quantidade × ValorCobrado)<br/>para cada peça vinculada
```

---

## 4. Diagrama de Sequência 3 — Movimentação de Estoque (Entrada)

### Descrição
Modela a interação durante o registro de uma entrada de peça no estoque, incluindo validações, atualização de saldo e criação de registro de auditoria.

### Referência no Código
- [EstoqueService.cs](file:///c:/projeto/mechsystem/Services/EstoqueService.cs)
- [MovimentacaoEstoque.cs](file:///c:/projeto/mechsystem/Models/MovimentacaoEstoque.cs)
- [Peca.cs](file:///c:/projeto/mechsystem/Models/Peca.cs)

### Diagrama

```mermaid
sequenceDiagram
    actor Admin as 👤 Administrador
    participant PG as Estoque.razor<br/>(Blazor Page)
    participant ES as EstoqueService
    participant DB as AppDbContext
    participant MOV as MovimentacaoEstoque<br/>(Model)

    Admin->>PG: Acessa módulo<br/>de estoque
    PG->>DB: SELECT * FROM Pecas<br/>WHERE Ativo = true
    DB-->>PG: List de Pecas
    PG-->>Admin: Exibe lista de peças<br/>com EstoqueAtual e<br/>indicador AbaixoDoMinimo

    Admin->>PG: Seleciona peça para<br/>movimentação
    PG-->>Admin: Exibe detalhes:<br/>SKU, Nome, Estoque Atual,<br/>Estoque Mínimo, Margem

    Admin->>PG: Seleciona tipo:<br/>"Entrada"
    Admin->>PG: Informa quantidade: 50
    Admin->>PG: Informa referência:<br/>"NF 12345 - Fornecedor X"

    Admin->>PG: Clica "Registrar"

    PG->>ES: RegistrarEntrada(pecaId,<br/>quantidade, referencia,<br/>usuarioId)

    ES->>DB: SELECT * FROM Pecas<br/>WHERE Id = @pecaId
    DB-->>ES: Peca encontrada<br/>(EstoqueAtual: 5)

    ES->>ES: NovoEstoque =<br/>EstoqueAtual + Quantidade<br/>(5 + 50 = 55)

    ES->>DB: UPDATE Pecas SET<br/>EstoqueAtual = 55<br/>WHERE Id = @pecaId

    ES->>MOV: Criar MovimentacaoEstoque
    Note over MOV: PecaId: @pecaId<br/>Tipo: Entrada (0)<br/>Quantidade: 50<br/>DataHora: DateTime.UtcNow<br/>Referencia: "NF 12345"<br/>UsuarioId: @adminId

    ES->>DB: INSERT MovimentacaoEstoque
    ES->>DB: SaveChangesAsync()

    DB-->>ES: Sucesso

    ES->>ES: Verificar:<br/>EstoqueAtual (55) ><br/>EstoqueMinimo (2)?
    Note over ES: 55 > 2 → OK<br/>Sem alerta

    ES-->>PG: Operação concluída<br/>com sucesso

    PG-->>Admin: Exibe confirmação:<br/>"Entrada registrada.<br/>Novo estoque: 55 un."

    PG->>DB: Recarregar lista<br/>atualizada
    DB-->>PG: Lista atualizada
    PG-->>Admin: Atualiza grid<br/>com novo estoque
```

### Cenário Alternativo — Saída com Alerta

```mermaid
sequenceDiagram
    actor Admin as 👤 Administrador
    participant ES as EstoqueService
    participant DB as AppDbContext

    Admin->>ES: RegistrarSaida(pecaId,<br/>quantidade: 8, referencia,<br/>usuarioId)

    ES->>DB: SELECT EstoqueAtual,<br/>EstoqueMinimo FROM Pecas
    DB-->>ES: EstoqueAtual: 10<br/>EstoqueMinimo: 5

    ES->>ES: 10 >= 8? Sim → OK

    ES->>DB: UPDATE EstoqueAtual = 2

    ES->>DB: INSERT MovimentacaoEstoque<br/>(Tipo: Saída)

    ES->>ES: Verificar: 2 <= 5?
    Note over ES: ⚠ ALERTA!<br/>EstoqueAtual (2) ≤<br/>EstoqueMinimo (5)

    ES-->>Admin: Operação concluída<br/>⚠ ALERTA: Peça abaixo<br/>do estoque mínimo!
```

---

## 5. Objetos Participantes (Resumo)

| Objeto | Tipo | Responsabilidade |
|--------|------|-----------------|
| Login.razor / CriarOS.razor / Estoque.razor | Blazor Component (UI) | Interação com usuário, validação de formulário |
| AuthEndpoints | Minimal API | Processar POST de login/logout |
| AuthService | Service | Validar credenciais, verificar BCrypt |
| EstoqueService | Service | Lógica de movimentação de estoque |
| AppDbContext | EF Core Context | Acesso ao banco de dados SQLite |
| OrdemServico, Peca, MovimentacaoEstoque | Model/Entity | Entidades de domínio |
| Cookie Authentication | Middleware | Gerenciar sessão autenticada |

---

## 6. Conclusão

Os 3 diagramas de sequência demonstram a interação entre as camadas do sistema:
1. **Login** — UI → API → Service → DB → Cookie
2. **Criação de OS** — UI → DB → Service (Estoque) → DB (transacional)
3. **Movimentação de Estoque** — UI → Service → DB + Auditoria

Cada diagrama é rastreável ao código-fonte real do MechSystem.

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
