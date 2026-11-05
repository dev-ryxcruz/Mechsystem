# Análise Técnica e Funcional — MechSystem (Atualizado Pós-Estabilização)

**Data da Auditoria Original:** Abril de 2026  
**Data da Atualização (Ciclo de Estabilização):** Outubro de 2026  
**Objetivo:** Registro do estado atual do sistema após a execução do plano de estabilização (Fases 1 a 4 na branch `chore/estabilizacao`), distinguindo os itens resolvidos das evoluções planejadas para pacotes futuros.

---

## 1. Estado Atual e Funcionalidades Estabilizadas (O que está em funcionamento)

* **Banco de Dados Relacional e Ambiente Reproduzível (`PostgreSQL 16` + Docker):**
  * Migrado e padronizado em **PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)** com convenção `snake_case`, todas as colunas `DateTime` padronizadas em UTC (`timestamp with time zone`) e exibição convertida para `America/Sao_Paulo` (`ToBrasiliaTime()`).
  * Disponibilizados `Dockerfile` multi-stage (.NET 10) e `docker-compose.yml` (app + PostgreSQL com healthcheck), além de pipeline CI em `.github/workflows/ci.yml`.
* **Máquina de Estados e Regras da Ordem de Serviço (`OrdemServicoService`):**
  * Transições de status centralizadas e validadas no servidor (`Orcamento` → `EmAndamento` ⇄ `Pausada` / `AguardandoPecas` → `Concluida`, e `Cancelada`), impedindo transições inválidas ou reabertura de O.S. concluída/cancelada.
  * Preenchimento automático de `DataConclusao` (UTC) ao concluir a O.S., utilizada como critério de competência financeira no Dashboard (`Home.razor`), na Central de Relatórios (`Relatorios/Index.razor`) e no cálculo de margem real (`EstoqueService.GetMargemLucroRealAsync`).
* **Controle de Estoque Único, Atômico e com Controle de Concorrência (`EstoqueService` + `PecaRepository`):**
  * Ponto único de verdade para saldo e histórico (`MovimentacaoEstoque`): peças em `Orcamento` não baixam estoque; aprovação baixa todas as peças em transação atômica (`BeginTransactionAsync`) com rollback integral se faltar saldo em qualquer item; adição/remoção em O.S. já aprovada baixa/estorna imediatamente; cancelamento de O.S. aprovada estorna o saldo líquido.
  * Eliminada a baixa duplicada que ocorria entre o Painel do Mecânico e a aprovação da O.S.
  * Token de concorrência otimista (`xmin` do PostgreSQL em `Peca.Version`) com tratamento amigável de `DbUpdateConcurrencyException`.
* **Segurança, Autorização no Servidor e Privacidade (LGPD):**
  * Validação de perfil no servidor para aprovar, cancelar, concluir, pausar/retomar e adicionar/remover peças; bloqueio server-side de desconto abaixo do `PrecoBase` (`PrecoVenda`) para não-administradores (inclusive ao incrementar peça já existente na O.S.).
  * Senha fixa removida do código e das migrations: bootstrap seguro via `Admin:InitialPassword` ou senha aleatória criptograficamente forte exibida uma única vez no `ILogger`.
  * Política mínima de senha em `UsuarioService`, Rate Limiting no `/api/auth/login` (5 tentativas/minuto por IP), cookie seguro (`SecurePolicy = Always` fora de Development) e revalidação de sessão (`OnValidatePrincipal`).
  * Portal público (`/acompanhar/{token}`) com tratamento correto do status `Pausada`, geração de token na criação da O.S. e **QR Code gerado localmente** via `QRCoder` (sem envio do link a serviços externos).
* **Integridade de Dados e Testes Automatizados (`mechsystem.Tests`):**
  * Índices únicos no banco (`ix_veiculos_placa`, `ix_clientes_cpf`, `ix_ordens_servico_token_acompanhamento`, `ix_pecas_sku`, `ix_usuarios_username`) com normalização e mensagens amigáveis.
  * Suíte xUnit cobrindo máquina de estados, fluxo completo de estoque sem baixa duplicada, rollback transacional por falta de estoque (InMemory e PostgreSQL real), bloqueio de desconto por perfil, cálculo de totais e `DataConclusao`.

---

## 2. Status dos Gaps Levantados na Auditoria Inicial

### 2.1. Paginação e Filtros nas Listagens — ✅ RESOLVIDO PARCIALMENTE (Filtros/Busca e Paginação nas listagens principais)
* **Resolvido:** As telas de Clientes (`Clientes/Index.razor`), Veículos (`Veiculos/Index.razor`) e Ordens de Serviço (`OrdensServico/Index.razor`) já contam com barra de busca textual (nome, CPF, placa, modelo, Nº da O.S.), filtro por status e paginação na interface.
* **Pendente (Melhoria futura de escala):** Refinar paginação `IQueryable` pura com `.Skip().Take()` direto no SQL para bases com dezenas de milhares de registros.

### 2.2. Controle e Registro de Estoque e Vínculo com O.S. — ✅ RESOLVIDO
* **Resolvido:** Módulo completo de Estoque (`Estoque/Index.razor`, `Estoque/Form.razor`, `Estoque/Entrada.razor`), vínculo tabelado `OrdemServicoPeca` com snapshot de preço base/custo, baixa/estorno transacional unificado em `EstoqueService` e `OrdemServicoService`, alerta de ruptura e histórico auditável por usuário.

### 2.3. Segurança de Autenticação, Autorização e QR Code — ✅ RESOLVIDO
* **Resolvido:** Remoção de senha fixa, `OnValidatePrincipal`, Rate Limiting no login, autorização no servidor por perfil, substituição de `api.qrserver.com` por `QRCoder` local e tratamento de exceções com `ILogger` + `DomainErrorHelper`.

### 2.4. Módulo Financeiro Detalhado (Contas a Receber / Caixa Diário) — ⏳ PENDENTE (Pacotes Futuros)
* A O.S. calcula os valores e margens reais por `DataConclusao`, mas o controle de **meios de pagamento (PIX, Cartão, parcelamento) e baixa de caixa/contas a receber** permanece planejado para evolução futura.

### 2.5. Histórico de Mudança de Status da O.S. (Audit Trail Visual) — ⏳ PENDENTE (Pacotes Futuros)
* O sistema registra `DataEmissao`, `DataAutorizacao`, `DataConclusao`, `ContatosOS` e `MovimentacaoEstoque` (com `UsuarioId` real do claim), mas uma tabela dedicada de log de transições de status da O.S. (timeline de auditoria) fica como evolução futura.

### 2.6. Integração com WhatsApp (`wa.me`) e Upload de Fotos na Vistoria — ⏳ PENDENTE (Pacotes Futuros / CRM)
* Mantidos fora do escopo de estabilização conforme diretriz de não implementar funcionalidades dos pacotes futuros antes da banca.
