# Proposta Comercial — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Elaborado por**: Ryan Cristian — Desenvolvimento de Software  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## PROPOSTA COMERCIAL Nº 2026/001

---

## 1. Apresentação

Prezado(a) cliente,

É com satisfação que apresentamos a proposta comercial para o desenvolvimento e implantação do **MechSystem** — uma solução completa de gestão para oficinas mecânicas. Este documento detalha o escopo, funcionalidades, investimento e condições comerciais do projeto.

O MechSystem foi projetado para **eliminar processos manuais**, **centralizar informações** e fornecer **inteligência de negócio** para a tomada de decisão, tudo em uma plataforma web moderna e acessível.

---

## 2. Dados do Fornecedor

| Campo | Informação |
|-------|-----------|
| **Razão Social** | Ryan Cristian — Desenvolvimento de Software |
| **CNPJ** | XX.XXX.XXX/0001-XX |
| **E-mail** | contato@mechsystem.com.br |
| **Telefone** | (XX) XXXX-XXXX |
| **LinkedIn** | [linkedin.com/in/ryan-cristian-a0889324b](https://www.linkedin.com/in/ryan-cristian-a0889324b) |
| **GitHub** | [github.com/dev-ryxcruz](https://github.com/dev-ryxcruz) |

---

## 3. Escopo da Solução

### 3.1 Módulos Incluídos

| # | Módulo | Descrição |
|---|--------|----------|
| 1 | **Autenticação e Autorização** | Login seguro com BCrypt, 3 perfis de acesso (Admin, Atendimento, Mecânico) |
| 2 | **Gestão de Clientes** | CRUD completo com CPF, contato e vinculação de veículos |
| 3 | **Gestão de Veículos** | Cadastro com placa, marca, modelo, ano e quilometragem |
| 4 | **Catálogo de Serviços** | Serviços com valor padrão e tempo estimado |
| 5 | **Ordens de Serviço** | Ciclo completo: Orçamento → Execução → Conclusão (5 estados) |
| 6 | **Vistoria de Entrada** | Checklist, nível de combustível, KM e mapeamento de avarias |
| 7 | **Controle de Estoque** | Peças com SKU, preço, estoque mínimo, movimentações e alertas |
| 8 | **Dashboard BI** | KPIs financeiros e operacionais em tempo real |
| 9 | **Relatórios** | Relatórios de OS, Financeiro e Estoque |
| 10 | **Configurações** | Dados da oficina, regras de negócio e parâmetros financeiros |
| 11 | **Impressão de OS** | Formatação profissional para impressão |
| 12 | **Acompanhamento** | Token para cliente acompanhar status da OS |

### 3.2 Tecnologias Utilizadas

| Camada | Tecnologia | Justificativa |
|--------|-----------|--------------|
| Backend | .NET 10 + ASP.NET Core | Performance, segurança, multiplataforma |
| Frontend | Blazor Server (Interactive) | SPA sem JavaScript, produtividade |
| Banco de Dados | SQLite (padrão) / PostgreSQL (escala) | Zero configuração + escalabilidade |
| ORM | Entity Framework Core 10 | Produtividade, migrations, type safety |
| Segurança | BCrypt + Cookie Auth | Padrão de mercado |
| Estilização | CSS Nativo | Sem dependências externas |

---

## 4. Investimento

### Opção A — Licença Perpétua (Compra Única)

| Item | Valor |
|------|-------|
| Licença do software (uso ilimitado) | R$ 4.500,00 |
| Instalação e configuração no ambiente do cliente | R$ 500,00 |
| Treinamento presencial ou remoto (4 horas) | R$ 500,00 |
| **Total (pagamento único)** | **R$ 5.500,00** |

> **Inclui**: 3 meses de suporte técnico gratuito após a implantação.  
> **Não inclui**: Customizações, integrações com sistemas externos, hospedagem em nuvem.

### Opção B — Assinatura Mensal (SaaS)

| Plano | Usuários | Funcionalidades | Valor/Mês |
|-------|---------|-----------------|-----------|
| **Starter** | Até 3 | Módulos 1-7 (sem BI e relatórios avançados) | R$ 99,00 |
| **Professional** | Até 10 | Todos os 12 módulos | R$ 199,00 |
| **Enterprise** | Ilimitado | Todos + suporte prioritário + customizações | R$ 349,00 |

> **Inclui**: Hospedagem, backup diário, atualizações, suporte técnico.  
> **Fidelidade mínima**: 12 meses (Starter/Professional), sem fidelidade (Enterprise).

### Opção C — Desenvolvimento Customizado

Para oficinas com necessidades específicas (integrações, módulos adicionais, white-label):

| Item | Valor Hora |
|------|-----------|
| Desenvolvimento de funcionalidades | R$ 120,00/h |
| Design UI/UX customizado | R$ 100,00/h |
| Integração com sistemas externos | R$ 150,00/h |
| Consultoria técnica | R$ 100,00/h |

---

## 5. Cronograma de Implantação

| Fase | Duração | Descrição |
|------|---------|----------|
| **1. Kickoff** | 1 dia | Reunião de alinhamento, levantamento de dados da oficina |
| **2. Configuração** | 1-2 dias | Instalação do sistema, configuração de parâmetros, criação de usuários |
| **3. Migração** | 2-3 dias | Importação de dados existentes (clientes, veículos, peças) — se aplicável |
| **4. Treinamento** | 1 dia | Treinamento dos operadores (atendimento e mecânico) |
| **5. Go-Live** | 1 dia | Acompanhamento no primeiro dia de operação |
| **Total** | **5-7 dias úteis** | |

---

## 6. SLA — Acordo de Nível de Serviço

### 6.1 Suporte Técnico

| Nível | Severidade | Tempo de Resposta | Tempo de Resolução |
|-------|-----------|-------------------|-------------------|
| **P1** | Sistema indisponível | 1 hora | 4 horas |
| **P2** | Funcionalidade crítica comprometida | 4 horas | 24 horas |
| **P3** | Funcionalidade não-crítica com defeito | 8 horas | 48 horas |
| **P4** | Dúvida ou melhoria | 24 horas | 5 dias úteis |

### 6.2 Disponibilidade (Plano SaaS)

| Métrica | Garantia |
|---------|---------|
| Uptime mensal | 99,5% |
| Janela de manutenção | Domingos, 02h-06h |
| Backup | Diário, retenção de 30 dias |
| Recuperação de desastres | RPO < 24h, RTO < 4h |

### 6.3 Canais de Suporte

| Canal | Disponibilidade | Planos |
|-------|----------------|--------|
| E-mail | 24/7 (resposta em horário comercial) | Todos |
| WhatsApp | Seg-Sex, 8h-18h | Professional + Enterprise |
| Telefone | Seg-Sex, 8h-18h | Enterprise |
| Acesso remoto | Mediante agendamento | Todos |

---

## 7. Condições Comerciais

### 7.1 Forma de Pagamento

**Licença Perpétua (Opção A):**
- 50% na assinatura do contrato
- 50% na entrega e aceite do sistema
- Ou: 3x sem juros no cartão

**Assinatura Mensal (Opção B):**
- Cobrança mensal via boleto, PIX ou cartão de crédito
- Vencimento todo dia 10

### 7.2 Validade da Proposta

Esta proposta é válida por **30 (trinta) dias** a partir da data de emissão.

### 7.3 Reajuste

Planos SaaS são reajustados anualmente pelo **IPCA** acumulado dos últimos 12 meses.

### 7.4 Cancelamento

- **Licença Perpétua**: Não reembolsável após a entrega e aceite.
- **SaaS com fidelidade**: Multa de 30% sobre as mensalidades restantes.
- **SaaS Enterprise**: Cancelamento a qualquer momento com 30 dias de aviso prévio.

---

## 8. Diferenciais Competitivos

| Diferencial | MechSystem | Concorrentes |
|------------|-----------|-------------|
| Banco de dados local (sem internet) | ✅ SQLite embutido | ❌ Exigem conexão |
| Vistoria de entrada documentada | ✅ Checklist + avarias | ⬜ Parcial |
| Dashboard com BI | ✅ 5+ KPIs em tempo real | ⬜ Básico |
| Controle RBAC (3 perfis) | ✅ Nativo | ⬜ Admin apenas |
| Portabilidade (Windows/Linux/Mac) | ✅ .NET multiplataforma | ❌ Geralmente Windows-only |
| Código moderno (.NET 10) | ✅ Stack 2026 | ❌ Legado (PHP, Delphi) |
| Custo mensal acessível | ✅ A partir de R$ 99 | ❌ R$ 200-500+ |
| Impressão profissional de OS | ✅ Nativo | ⬜ Módulo adicional pago |
| Token de acompanhamento | ✅ Cliente consulta status | ❌ Não disponível |

---

## 9. Garantia

| Item | Garantia |
|------|---------|
| Correção de bugs | 12 meses (Licença) / Vitalícia (SaaS) |
| Atualizações de segurança | 12 meses (Licença) / Incluído (SaaS) |
| Atualizações de funcionalidade | Sob consulta (Licença) / Incluído (SaaS) |
| Garantia de funcionamento | Conforme especificações documentadas |

---

## 10. Aceite

Para prosseguir com a contratação, favor assinar abaixo e enviar este documento digitalizado para o e-mail indicado.

| | Cliente | Fornecedor |
|---|---------|-----------|
| **Nome** | _________________________ | Ryan Cristian |
| **Cargo** | _________________________ | Desenvolvedor / Gerente de Projeto |
| **Data** | ____/____/2026 | ____/____/2026 |
| **Assinatura** | _________________________ | _________________________ |

---

**Opção escolhida**: ( ) Opção A — Licença Perpétua | ( ) Opção B — SaaS __________ | ( ) Opção C — Customizado

---

*Proposta elaborada como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
