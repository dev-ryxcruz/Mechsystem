# Missão, Visão e Valor — MechSystem

**Projeto**: MechSystem — Sistema de Gestão para Oficinas Mecânicas  
**Versão**: 1.0  
**Data**: Maio/2026  
**Autor**: Ryan Cristian  
**Disciplina**: Engenharia de Software III — Prof. Alessandro Fukuta

---

## 1. Missão

> **Digitalizar e otimizar a gestão operacional de oficinas mecânicas**, oferecendo uma plataforma web moderna, acessível e integrada que automatiza processos de atendimento, ordens de serviço, controle de estoque e gestão financeira — permitindo que proprietários e colaboradores foquem no que fazem de melhor: cuidar dos veículos de seus clientes.

### Detalhamento da Missão

- **Eliminar processos manuais e em papel** que geram retrabalho, erros e perda de informações.
- **Centralizar dados operacionais** (clientes, veículos, peças, serviços) em um único sistema acessível via navegador.
- **Garantir conformidade legal**, especialmente com o Código de Defesa do Consumidor (CDC), através de orçamentos formais, vistorias documentadas e rastreabilidade completa.
- **Democratizar o acesso à tecnologia** para oficinas de todos os portes, com licenciamento acessível e implantação simplificada.

---

## 2. Visão

> **Ser a plataforma de referência nacional para gestão de oficinas mecânicas até 2028**, reconhecida pela excelência em usabilidade, inteligência operacional e custo-benefício — transformando oficinas tradicionais em negócios digitalmente maduros e competitivos.

### Metas Estratégicas de Longo Prazo

| Horizonte | Meta |
|-----------|------|
| **2026** | Lançamento da versão 1.0 com módulos de OS, estoque, clientes e dashboard BI |
| **2027** | Integração com gateways de pagamento, notificações automáticas e app mobile |
| **2028** | Marketplace de peças integrado, IA para diagnóstico preditivo e expansão para redes de franquias |

---

## 3. Valores

### 3.1 Simplicidade com Profundidade

O sistema deve ser **intuitivo na superfície**, mas **robusto por dentro**. Qualquer colaborador deve conseguir operar a ferramenta com treinamento mínimo, enquanto o administrador tem acesso a configurações avançadas e relatórios estratégicos.

### 3.2 Confiabilidade

Os dados dos clientes e das operações são **críticos para o negócio**. O MechSystem preza pela integridade dos dados, backups automáticos, controle de acesso por perfis (RBAC) e criptografia de senhas com BCrypt.

### 3.3 Transparência

Toda operação deve ser **rastreável e auditável**: desde a entrada do veículo (vistoria), passando pela autorização do serviço, até a conclusão e entrega. O cliente pode acompanhar o status da OS via token de acompanhamento.

### 3.4 Inovação Contínua

O projeto utiliza tecnologias de ponta (.NET 10, Blazor Server, Entity Framework Core 10) e segue práticas modernas de engenharia de software: Clean Architecture, Injeção de Dependências, padrão Repository e separação clara de responsabilidades.

### 3.5 Foco no Cliente Final

O dono da oficina é nosso cliente direto, mas o consumidor final (dono do veículo) é impactado indiretamente. Funcionalidades como acompanhamento de OS online, vistorias fotográficas e comunicação registrada refletem o compromisso com a experiência do consumidor.

### 3.6 Acessibilidade Econômica

O MechSystem é projetado para ser **economicamente viável para micro e pequenas oficinas**, utilizando SQLite como banco de dados embutido (zero configuração) e podendo escalar para PostgreSQL conforme o crescimento do negócio.

---

## Síntese Visual

```
┌───────────────────────────────────────────────────────────────────┐
│                         MECHSYSTEM                                │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│  MISSÃO     → Digitalizar e otimizar a gestão de oficinas         │
│               mecânicas com uma plataforma web moderna            │
│                                                                   │
│  VISÃO      → Ser referência nacional em gestão de oficinas       │
│               mecânicas até 2028                                  │
│                                                                   │
│  VALORES    → Simplicidade · Confiabilidade · Transparência       │
│               Inovação · Foco no Cliente · Acessibilidade         │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

---

*Documento elaborado como artefato da disciplina de Engenharia de Software III — FATEC 2026/1*
