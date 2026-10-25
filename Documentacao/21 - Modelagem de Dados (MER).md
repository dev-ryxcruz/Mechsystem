# Projeto Semestral — Etapa 1: Modelagem de Dados

**Sistema modelado:** MechSystem (gestão de oficina mecânica) — base: Projeto Integrador do semestre.

> Os diagramas abaixo são Mermaid. Para visualizar: preview do VS Code (extensão *Markdown Preview Mermaid Support*), GitHub, ou colar o código em <https://mermaid.live> para exportar PNG/PDF.

---

## 1. Definição do contexto do sistema

### 1.1 Nome / tema
**MechSystem** — sistema web para gestão de oficinas mecânicas (clientes, veículos, ordens de serviço, vistoria de entrada e controle de estoque de peças).

### 1.2 Problema / necessidade
Oficinas pequenas e médias controlam orçamentos, ordens de serviço (OS) e estoque em papel ou planilhas. Isso gera:
- perda de histórico do cliente e do veículo;
- orçamentos sem registro de autorização (risco jurídico — CDC);
- divergência entre estoque físico e real de peças;
- falta de rastreio de quem alterou preços ou deu desconto;
- ausência de registro do estado do veículo na entrada (disputas por avarias).

O sistema centraliza esses dados e garante que as regras da oficina sejam respeitadas.

### 1.3 Principais usuários / envolvidos
| Envolvido | Papel |
|---|---|
| **Administrador** | Configura a oficina, gerencia usuários, autoriza descontos |
| **Atendimento** | Cadastra clientes/veículos, abre orçamentos e OS, registra contatos |
| **Mecânico** | Executa serviços, registra tempo gasto, consome peças |
| **Cliente** | Contrata serviços, autoriza orçamentos, acompanha a OS por token |

### 1.4 Principais funcionalidades relacionadas aos dados
- Cadastro de clientes e seus veículos.
- Catálogo de serviços (valor padrão e tempo estimado) e de peças (SKU, custo, venda, estoque).
- Abertura de OS com peças e serviços, cálculo de total e controle de status.
- Vistoria de entrada (combustível, KM, itens do veículo, avarias).
- Registro de contatos com o cliente sobre a OS.
- Movimentação de estoque (entrada, saída, ajuste) com responsável.
- Controle de acesso por perfil e parâmetros globais da oficina.

### 1.5 Regras de negócio consideradas na modelagem
| # | Regra |
|---|---|
| RN01 | Um cliente pode ter vários veículos, mas cada veículo pertence a apenas **um** cliente. |
| RN02 | O CPF do cliente é obrigatório e **único**; a placa do veículo é obrigatória e **única**. |
| RN03 | Um veículo pode ter várias ordens de serviço ao longo do tempo; cada OS refere-se a **um único** veículo. |
| RN04 | Toda OS exige problema relatado e **data de previsão de entrega** (CDC), que não pode ser anterior à emissão. |
| RN05 | Uma OS tem no máximo **uma** vistoria de entrada; cada vistoria pertence a uma única OS. (A obrigatoriedade é parametrizável em `Configuracao`.) |
| RN06 | Uma OS pode conter várias peças e vários serviços; a mesma peça/serviço pode aparecer em várias OS, com **quantidade** e **valores próprios** por OS (relacionamentos N:M com atributos → entidades associativas). |
| RN07 | Ao incluir peça/serviço na OS, os valores de catálogo (preço base, custo, tempo) são **copiados** (snapshot); alterar o catálogo depois não altera OS antigas. |
| RN08 | Valor cobrado abaixo do preço base é desconto e exige perfil **Administrador**. |
| RN09 | Quantidades em OS e movimentações devem ser **≥ 1**; estoque atual e mínimo **não podem ser negativos**. |
| RN10 | O SKU da peça é único. Peça com movimentação ou usada em OS **não pode ser excluída** (restrição). |
| RN11 | Toda movimentação de estoque pertence a **uma** peça e é feita por **um** usuário (rastreabilidade). |
| RN12 | O nome de usuário é único; cada usuário tem exatamente **um** perfil (Administrador, Atendimento, Mecânico). |
| RN13 | Cada OS possui status único controlado por máquina de estados: `Orcamento`, `EmAndamento`, `Pausada`, `AguardandoPecas`, `Concluida` ou `Cancelada`. |
| RN14 | Uma OS pode ter vários registros de contato com o cliente (ligação, WhatsApp, e-mail, presencial, outro). |
| RN15 | Excluir uma OS remove seus itens (peças e serviços) em cascata. |
| RN16 | Existe **um único** registro de configuração da oficina (nome, CNPJ, validade do orçamento, garantia padrão etc.). |
| RN17 | Valor total da OS = mão de obra + peças − desconto (**atributo derivado**, não armazenado). |

> **Exemplo (como a regra aparece nos diagramas):** RN01 → relacionamento `CLIENTE (1,1) — possui — (0,N) VEICULO` no conceitual e `veiculo.cliente_id FK NOT NULL` no lógico.

---

## 2. Diagrama Conceitual (DER)

Notação de cardinalidade (mín,máx) lida a partir de cada entidade:

| Símbolo Mermaid | Significado |
|---|---|
| `\|\|` | exatamente um (1,1) |
| `o\|` | zero ou um (0,1) |
| `o{` | zero ou muitos (0,N) |
| `\|{` | um ou muitos (1,N) |

Entidades associativas: **ITEM_PECA_OS** (OS × PECA) e **ITEM_SERVICO_OS** (OS × SERVICO). **MOVIMENTACAO_ESTOQUE** também é associativa (PECA × USUARIO), com identidade própria por poder repetir-se.

```mermaid
erDiagram
    CLIENTE ||--o{ VEICULO : "possui"
    VEICULO ||--o{ ORDEM_SERVICO : "é atendido em"
    ORDEM_SERVICO ||--o| VISTORIA : "tem"
    ORDEM_SERVICO ||--o{ CONTATO_OS : "registra"
    ORDEM_SERVICO ||--o{ ITEM_PECA_OS : "consome"
    PECA ||--o{ ITEM_PECA_OS : "é usada em"
    ORDEM_SERVICO ||--o{ ITEM_SERVICO_OS : "executa"
    SERVICO ||--o{ ITEM_SERVICO_OS : "é executado em"
    PECA ||--o{ MOVIMENTACAO_ESTOQUE : "sofre"
    USUARIO ||--o{ MOVIMENTACAO_ESTOQUE : "realiza"

    CLIENTE {
        int id PK "identificador"
        string nome
        string cpf "identificador alternativo"
        string email
        string telefone
        string endereco
        boolean ativo
    }
    VEICULO {
        int id PK "identificador"
        string placa "identificador alternativo"
        string marca
        string modelo
        string cor
        int ano
        int quilometragem
    }
    ORDEM_SERVICO {
        int id PK "identificador"
        datetime data_emissao
        datetime data_previsao_inicio
        datetime data_previsao_entrega
        string problema_relatado
        string status
        decimal valor_mao_de_obra
        decimal valor_pecas
        decimal valor_desconto
        int tempo_gasto_minutos
        string autorizado_por
        string meio_autorizacao
        datetime data_autorizacao
        string token_acompanhamento
        decimal valor_total "derivado"
    }
    VISTORIA {
        int id PK "identificador"
        string status
        string nivel_combustivel
        int quilometragem_entrada
        boolean tem_estepe
        boolean tem_macaco
        boolean tem_radio
        boolean tem_triangulo
        boolean tem_chave_roda
        string avarias_json
        string observacoes
        datetime data_vistoria
    }
    CONTATO_OS {
        int id PK "identificador"
        datetime data_contato
        string tipo
        string descricao
        string registrado_por
    }
    PECA {
        int id PK "identificador"
        string sku "identificador alternativo"
        string nome
        string marca
        decimal preco_custo
        decimal preco_venda
        int estoque_atual
        int estoque_minimo
        string localizacao
        boolean ativo
    }
    SERVICO {
        int id PK "identificador"
        string nome
        string descricao
        decimal valor_padrao
        int tempo_estimado_minutos
    }
    ITEM_PECA_OS {
        int quantidade
        decimal preco_base
        decimal valor_cobrado
        decimal preco_custo_snapshot
    }
    ITEM_SERVICO_OS {
        int quantidade
        decimal preco_base
        decimal valor_cobrado
        int tempo_estimado_snapshot
    }
    MOVIMENTACAO_ESTOQUE {
        int id PK "identificador"
        string tipo
        int quantidade
        datetime data_hora
        string referencia
    }
    USUARIO {
        int id PK "identificador"
        string username "identificador alternativo"
        string senha_hash
        string nome_completo
        string perfil
        boolean ativo
        datetime data_criacao
    }
    CONFIGURACAO {
        int id PK "registro único"
        string nome_fantasia
        string cnpj
        string telefone
        string whatsapp
        string email
        string endereco_completo
        string mensagem_rodape
        int validade_orcamento_dias
        int garantia_padrao_dias
        boolean obrigar_vistoria_para_os
        string simbolo_moeda
        decimal taxa_mao_de_obra
    }
```

### Cardinalidades (mín,máx)

| Relacionamento | Lado A | Lado B | Regra |
|---|---|---|---|
| CLIENTE **possui** VEICULO | CLIENTE (0,N) | VEICULO (1,1) | RN01 |
| VEICULO **é atendido em** ORDEM_SERVICO | VEICULO (0,N) | ORDEM_SERVICO (1,1) | RN03 |
| ORDEM_SERVICO **tem** VISTORIA | ORDEM_SERVICO (0,1) | VISTORIA (1,1) | RN05 |
| ORDEM_SERVICO **registra** CONTATO_OS | ORDEM_SERVICO (0,N) | CONTATO_OS (1,1) | RN14 |
| ORDEM_SERVICO **consome** PECA (assoc. ITEM_PECA_OS) | ORDEM_SERVICO (0,N) | PECA (0,N) | RN06 |
| ORDEM_SERVICO **executa** SERVICO (assoc. ITEM_SERVICO_OS) | ORDEM_SERVICO (0,N) | SERVICO (0,N) | RN06 |
| PECA **sofre** MOVIMENTACAO_ESTOQUE | PECA (0,N) | MOVIMENTACAO (1,1) | RN11 |
| USUARIO **realiza** MOVIMENTACAO_ESTOQUE | USUARIO (0,N) | MOVIMENTACAO (1,1) | RN11 |
| CONFIGURACAO | sem relacionamentos (parâmetros globais, registro único) | | RN16 |

---

## 3. Diagrama Lógico (Modelo Relacional)

Convenções: `PK` chave primária · `FK` chave estrangeira · `UK` restrição UNIQUE.
Valores `decimal` = `DECIMAL(18,2)`, exceto `taxa_mao_de_obra` (percentual 0–100). Campos de enum são armazenados como `INT`. `avarias_json` é `TEXT` com JSON.

```mermaid
erDiagram
    CLIENTE ||--o{ VEICULO : "cliente_id"
    VEICULO ||--o{ ORDEM_SERVICO : "veiculo_id"
    ORDEM_SERVICO ||--o| VISTORIA : "ordem_servico_id"
    ORDEM_SERVICO ||--o{ CONTATO_OS : "ordem_servico_id"
    ORDEM_SERVICO ||--o{ ORDEM_SERVICO_PECA : "ordem_servico_id"
    PECA ||--o{ ORDEM_SERVICO_PECA : "peca_id"
    ORDEM_SERVICO ||--o{ ORDEM_SERVICO_SERVICO : "ordem_servico_id"
    SERVICO ||--o{ ORDEM_SERVICO_SERVICO : "servico_id"
    PECA ||--o{ MOVIMENTACAO_ESTOQUE : "peca_id"
    USUARIO ||--o{ MOVIMENTACAO_ESTOQUE : "usuario_id"

    CLIENTE {
        INT id PK
        VARCHAR100 nome "NOT NULL"
        VARCHAR14 cpf UK "NOT NULL"
        VARCHAR100 email
        VARCHAR20 telefone
        VARCHAR300 endereco
        BOOLEAN ativo "NOT NULL DEFAULT true"
    }
    VEICULO {
        INT id PK
        INT cliente_id FK "NOT NULL"
        VARCHAR10 placa UK "NOT NULL"
        VARCHAR60 marca
        VARCHAR60 modelo
        VARCHAR30 cor
        INT ano "CHECK 1950..2100"
        INT quilometragem "CHECK >= 0"
    }
    ORDEM_SERVICO {
        INT id PK
        INT veiculo_id FK "NOT NULL"
        TIMESTAMP data_emissao "NOT NULL"
        TIMESTAMP data_previsao_inicio
        TIMESTAMP data_previsao_entrega "NOT NULL CHECK >= data_emissao"
        TEXT descricao_problema_relatado "NOT NULL"
        INT status "NOT NULL DEFAULT 0"
        DECIMAL valor_mao_de_obra "NOT NULL CHECK >= 0"
        DECIMAL valor_pecas "NOT NULL CHECK >= 0"
        DECIMAL valor_desconto "NOT NULL CHECK >= 0"
        INT tempo_gasto_minutos "NOT NULL DEFAULT 0"
        TIMESTAMP inicio_ultima_execucao
        VARCHAR150 autorizado_por
        VARCHAR30 meio_autorizacao
        TIMESTAMP data_autorizacao
        VARCHAR64 token_acompanhamento UK
    }
    VISTORIA {
        INT id PK
        INT ordem_servico_id FK "UK NOT NULL"
        INT status "NOT NULL DEFAULT 0"
        INT nivel_combustivel "NOT NULL CHECK 1..5"
        INT quilometragem_entrada "NOT NULL CHECK >= 0"
        BOOLEAN tem_estepe "NOT NULL"
        BOOLEAN tem_macaco "NOT NULL"
        BOOLEAN tem_radio "NOT NULL"
        BOOLEAN tem_triangulo "NOT NULL"
        BOOLEAN tem_chave_roda "NOT NULL"
        TEXT avarias_json
        TEXT observacoes
        TIMESTAMP data_vistoria "NOT NULL"
    }
    CONTATO_OS {
        INT id PK
        INT ordem_servico_id FK "NOT NULL"
        TIMESTAMP data_contato "NOT NULL"
        INT tipo "NOT NULL"
        TEXT descricao "NOT NULL"
        VARCHAR100 registrado_por "NOT NULL"
    }
    PECA {
        INT id PK
        VARCHAR50 sku UK "NOT NULL"
        VARCHAR150 nome "NOT NULL"
        VARCHAR100 marca
        DECIMAL preco_custo "NOT NULL CHECK >= 0"
        DECIMAL preco_venda "NOT NULL CHECK >= 0"
        INT estoque_atual "NOT NULL CHECK >= 0"
        INT estoque_minimo "NOT NULL CHECK >= 0"
        VARCHAR200 localizacao
        BOOLEAN ativo "NOT NULL DEFAULT true"
    }
    SERVICO {
        INT id PK
        VARCHAR100 nome "NOT NULL"
        VARCHAR500 descricao
        DECIMAL valor_padrao "NOT NULL CHECK >= 0"
        INT tempo_estimado_minutos "CHECK 0..10000"
    }
    ORDEM_SERVICO_PECA {
        INT id PK
        INT ordem_servico_id FK "NOT NULL"
        INT peca_id FK "NOT NULL"
        INT quantidade "NOT NULL CHECK >= 1"
        DECIMAL preco_base "NOT NULL"
        DECIMAL valor_cobrado "NOT NULL CHECK >= 0"
        DECIMAL preco_custo_snapshot "NOT NULL"
    }
    ORDEM_SERVICO_SERVICO {
        INT id PK
        INT ordem_servico_id FK "NOT NULL"
        INT servico_id FK "NOT NULL"
        INT quantidade "NOT NULL CHECK >= 1"
        DECIMAL preco_base "NOT NULL"
        DECIMAL valor_cobrado "NOT NULL CHECK >= 0"
        INT tempo_estimado_snapshot
    }
    MOVIMENTACAO_ESTOQUE {
        INT id PK
        INT peca_id FK "NOT NULL"
        INT usuario_id FK "NOT NULL"
        INT tipo "NOT NULL 0=Entrada 1=Saida 2=Ajuste"
        INT quantidade "NOT NULL CHECK >= 1"
        TIMESTAMP data_hora "NOT NULL"
        VARCHAR300 referencia
    }
    USUARIO {
        INT id PK
        VARCHAR50 username UK "NOT NULL"
        VARCHAR255 senha_hash "NOT NULL"
        VARCHAR100 nome_completo
        INT perfil "NOT NULL 1=Admin 2=Atendimento 3=Mecanico"
        BOOLEAN ativo "NOT NULL DEFAULT true"
        TIMESTAMP data_criacao "NOT NULL"
    }
    CONFIGURACAO {
        INT id PK
        VARCHAR150 nome_fantasia "NOT NULL"
        VARCHAR20 cnpj
        VARCHAR20 telefone
        VARCHAR20 whatsapp
        VARCHAR100 email
        VARCHAR300 endereco_completo
        VARCHAR500 mensagem_rodape
        INT validade_orcamento_dias "NOT NULL CHECK 1..365"
        INT garantia_padrao_dias "NOT NULL CHECK 1..3650"
        BOOLEAN obrigar_vistoria_para_os "NOT NULL"
        VARCHAR10 simbolo_moeda "NOT NULL"
        DECIMAL taxa_mao_de_obra "CHECK 0..100"
    }
```

### 3.1 Restrições adicionais (UNIQUE compostas e regras)

| Tabela | Restrição | Motivo |
|---|---|---|
| `ordem_servico_peca` | `UNIQUE (ordem_servico_id, peca_id)` | evita a mesma peça repetida em linhas na mesma OS (a quantidade controla o volume) — RN06 |
| `ordem_servico_servico` | `UNIQUE (ordem_servico_id, servico_id)` | idem para serviços — RN06 |
| `vistoria` | `UNIQUE (ordem_servico_id)` | garante a cardinalidade (0,1) da OS — RN05 |
| `cliente` | `UNIQUE (cpf)` | RN02 |
| `veiculo` | `UNIQUE (placa)` | RN02 |
| `peca` | `UNIQUE (sku)` | RN10 |
| `usuario` | `UNIQUE (username)` | RN12 |

### 3.2 Ações referenciais (ON DELETE)

| FK | Ação | Regra |
|---|---|---|
| `ordem_servico_peca.ordem_servico_id` | **CASCADE** | RN15 |
| `ordem_servico_servico.ordem_servico_id` | **CASCADE** | RN15 |
| `ordem_servico_peca.peca_id` | **RESTRICT** | RN10 |
| `movimentacao_estoque.peca_id` | **RESTRICT** | RN10 |
| `movimentacao_estoque.usuario_id` | **RESTRICT** | RN11 |
| `veiculo.cliente_id`, `ordem_servico.veiculo_id`, `vistoria.ordem_servico_id`, `contato_os.ordem_servico_id`, `ordem_servico_servico.servico_id` | RESTRICT (padrão) | preservar histórico |

### 3.3 Atributos derivados (não armazenados)
- `ordem_servico.valor_total = (mão de obra efetiva + peças efetivas) − desconto`
- `Subtotal` do item = `quantidade × valor_cobrado`
- Margem de lucro da peça = `(preco_venda − preco_custo) / preco_venda × 100`

---

## 4. Coerência entre os modelos

```
Regras de negócio  →  Modelo Conceitual  →  Modelo Lógico
```

| Conceitual | Lógico | Transformação |
|---|---|---|
| Entidade | Tabela | 1 para 1 |
| Atributo identificador | PK | `id` surrogate; identificadores naturais (CPF, placa, SKU, username) viram `UNIQUE` |
| Relacionamento 1:N | FK no lado N | `VEICULO.cliente_id`, `ORDEM_SERVICO.veiculo_id`, `CONTATO_OS.ordem_servico_id`, `MOVIMENTACAO_ESTOQUE.peca_id/usuario_id` |
| Relacionamento 1:(0,1) | FK + `UNIQUE` | `VISTORIA.ordem_servico_id UNIQUE` |
| Relacionamento N:M com atributos | Entidade associativa → tabela com 2 FKs | `ITEM_PECA_OS` → `ORDEM_SERVICO_PECA`; `ITEM_SERVICO_OS` → `ORDEM_SERVICO_SERVICO` |
| Cardinalidade mínima 1 (obrigatória) | `NOT NULL` na FK | todas as FKs |
| Atributo derivado | não persistido | `valor_total` |
| Entidade sem relacionamento | Tabela isolada | `CONFIGURACAO` |

---

## 5. Observações de aderência ao código atual (Pós-Estabilização)

Modelo derivado das entidades em `Models/` e do [AppDbContext.cs](../Data/AppDbContext.cs). Com o ciclo de estabilização:
- `cliente.cpf`, `veiculo.placa` e `ordem_servico.token_acompanhamento` possuem índices `UNIQUE` no banco PostgreSQL (além de normalização automática na aplicação).
- `ordem_servico.data_conclusao` foi adicionada (`TIMESTAMPTZ NULL`) e preenchida automaticamente na conclusão da OS para apuração financeira por competência de conclusão.
- `peca.xmin` (`Version`) foi configurado como token nativo de concorrência otimista do PostgreSQL (`IsRowVersion()`).
- Todas as colunas `DateTime` são persistidas em UTC (`timestamp with time zone`) via convenção global no `AppDbContext`.

Fora do escopo de tabelas próprias: classes `Relatorio*` (consultas/relatórios; não possuem `DbSet` no contexto) e `Login` (DTO de autenticação).

---

## 6. Roteiro sugerido para a apresentação (3–5 min)

1. **Contexto (1 min):** nome, problema, usuários, funcionalidades (seção 1).
2. **Regras de negócio (1 min):** destacar RN01, RN05, RN06, RN07 e RN08.
3. **Conceitual (1–2 min):** entidades, identificadores, cardinalidades e as 2 associativas.
4. **Lógico (1 min):** mostrar a conversão (seção 4), PKs/FKs, `UNIQUE`, `NOT NULL` e `ON DELETE`.
5. **Coerência:** demonstrar RN01 do conceitual ao lógico.
