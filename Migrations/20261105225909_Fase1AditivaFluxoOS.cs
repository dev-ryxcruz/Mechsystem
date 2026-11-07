using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class Fase1AditivaFluxoOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "descricao_problema_relatado",
                table: "ordens_servico",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "aprovado_por_usuario_id",
                table: "ordens_servico",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_entrega",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "diagnostico",
                table: "ordens_servico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "garantia_dias",
                table: "ordens_servico",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "km_entrada",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "km_saida",
                table: "ordens_servico",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mecanico_responsavel_id",
                table: "ordens_servico",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nivel_combustivel_entrada",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "queixa_cliente",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "tipo_contato_autorizacao",
                table: "ordens_servico",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "validade_orcamento_ate",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "eh_aditivo",
                table: "ordem_servico_servicos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "executado",
                table: "ordem_servico_servicos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "mecanico_id",
                table: "ordem_servico_servicos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status_item",
                table: "ordem_servico_servicos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "aplicada",
                table: "ordem_servico_pecas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "eh_aditivo",
                table: "ordem_servico_pecas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "origem",
                table: "ordem_servico_pecas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status_item",
                table: "ordem_servico_pecas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "apontamentos_tempo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<int>(type: "integer", nullable: false),
                    ordem_servico_servico_id = table.Column<int>(type: "integer", nullable: true),
                    mecanico_id = table.Column<int>(type: "integer", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apontamentos_tempo", x => x.id);
                    table.ForeignKey(
                        name: "fk_apontamentos_tempo_ordem_servico_servicos_ordem_servico_ser",
                        column: x => x.ordem_servico_servico_id,
                        principalTable: "ordem_servico_servicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_apontamentos_tempo_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_apontamentos_tempo_usuarios_mecanico_id",
                        column: x => x.mecanico_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ordem_servico_id = table.Column<int>(type: "integer", nullable: false),
                    forma = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    registrado_por_usuario_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "fk_pagamentos_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pagamentos_usuarios_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_aprovado_por_usuario_id",
                table: "ordens_servico",
                column: "aprovado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_mecanico_responsavel_id",
                table: "ordens_servico",
                column: "mecanico_responsavel_id");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_servicos_mecanico_id",
                table: "ordem_servico_servicos",
                column: "mecanico_id");

            migrationBuilder.CreateIndex(
                name: "ix_apontamentos_tempo_mecanico_id_fim",
                table: "apontamentos_tempo",
                columns: new[] { "mecanico_id", "fim" },
                filter: "fim IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_apontamentos_tempo_ordem_servico_id",
                table: "apontamentos_tempo",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_apontamentos_tempo_ordem_servico_servico_id",
                table: "apontamentos_tempo",
                column: "ordem_servico_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_ordem_servico_id",
                table: "pagamentos",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_registrado_por_usuario_id",
                table: "pagamentos",
                column: "registrado_por_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ordem_servico_servicos_usuarios_mecanico_id",
                table: "ordem_servico_servicos",
                column: "mecanico_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ordens_servico_usuarios_aprovado_por_usuario_id",
                table: "ordens_servico",
                column: "aprovado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ordens_servico_usuarios_mecanico_responsavel_id",
                table: "ordens_servico",
                column: "mecanico_responsavel_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // ─── Script de dados idempotente (Fase 1) ────────────────────────────────
            migrationBuilder.Sql(@"
-- 1. Copiar DescricaoProblemaRelatado para QueixaCliente
UPDATE ordens_servico
SET queixa_cliente = COALESCE(NULLIF(BTRIM(descricao_problema_relatado), ''), 'Sem queixa registrada (migrado)')
WHERE queixa_cliente IS NULL OR BTRIM(queixa_cliente) = '';

-- 2. O.S. sem linhas de serviço e com ValorMaoDeObra > 0: criar linha genérica ""Mão de obra (migrado)""
INSERT INTO servicos (nome, descricao, valor_padrao, tempo_estimado_minutos, ativo)
SELECT 'Mão de obra (migrado)',
       'Serviço genérico criado automaticamente na migração da Fase 1 para preservar ValorMaoDeObra legado.',
       0,
       60,
       FALSE
WHERE EXISTS (
    SELECT 1
    FROM ordens_servico o
    WHERE o.valor_mao_de_obra > 0
      AND NOT EXISTS (SELECT 1 FROM ordem_servico_servicos oss WHERE oss.ordem_servico_id = o.id)
)
AND NOT EXISTS (
    SELECT 1 FROM servicos s WHERE s.nome = 'Mão de obra (migrado)'
);

INSERT INTO ordem_servico_servicos (
    ordem_servico_id,
    servico_id,
    quantidade,
    preco_base,
    valor_cobrado,
    tempo_estimado_minutos_snapshot,
    status_item,
    executado,
    eh_aditivo
)
SELECT o.id,
       (SELECT s.id FROM servicos s WHERE s.nome = 'Mão de obra (migrado)' ORDER BY s.id LIMIT 1),
       1,
       o.valor_mao_de_obra,
       o.valor_mao_de_obra,
       60,
       CASE WHEN o.status IN (0, 4, 9) THEN 0 ELSE 1 END,
       CASE WHEN o.status IN (3, 8) THEN TRUE ELSE FALSE END,
       FALSE
FROM ordens_servico o
WHERE o.valor_mao_de_obra > 0
  AND NOT EXISTS (SELECT 1 FROM ordem_servico_servicos oss WHERE oss.ordem_servico_id = o.id);

-- 3. O.S. sem linhas de peça e com ValorPecas > 0: criar linha de peça genérica ""Peças diversas (migrado)""
INSERT INTO pecas (sku, nome, marca, preco_custo, preco_venda, estoque_atual, estoque_minimo, localizacao, ativo)
SELECT 'MIGRADO-PECA-GEN',
       'Peças diversas (migrado)',
       'Genérica',
       0,
       0,
       0,
       0,
       'N/A',
       FALSE
WHERE EXISTS (
    SELECT 1
    FROM ordens_servico o
    WHERE o.valor_pecas > 0
      AND NOT EXISTS (SELECT 1 FROM ordem_servico_pecas osp WHERE osp.ordem_servico_id = o.id)
)
AND NOT EXISTS (
    SELECT 1 FROM pecas p WHERE p.sku = 'MIGRADO-PECA-GEN'
);

INSERT INTO ordem_servico_pecas (
    ordem_servico_id,
    peca_id,
    quantidade,
    preco_base,
    valor_cobrado,
    preco_custo_snapshot,
    origem,
    status_item,
    aplicada,
    eh_aditivo
)
SELECT o.id,
       (SELECT p.id FROM pecas p WHERE p.sku = 'MIGRADO-PECA-GEN' ORDER BY p.id LIMIT 1),
       1,
       o.valor_pecas,
       o.valor_pecas,
       0,
       1,
       CASE WHEN o.status IN (0, 4, 9) THEN 0 ELSE 1 END,
       CASE WHEN o.status IN (3, 8) THEN TRUE ELSE FALSE END,
       FALSE
FROM ordens_servico o
WHERE o.valor_pecas > 0
  AND NOT EXISTS (SELECT 1 FROM ordem_servico_pecas osp WHERE osp.ordem_servico_id = o.id);

-- Atualizar status_item e flags de execução/aplicação das linhas já existentes em O.S. aprovadas/concluídas
UPDATE ordem_servico_servicos oss
SET status_item = 1,
    executado = CASE WHEN o.status IN (3, 8) THEN TRUE ELSE oss.executado END
FROM ordens_servico o
WHERE oss.ordem_servico_id = o.id
  AND o.status IN (1, 2, 3, 5, 6, 7, 8)
  AND oss.status_item = 0;

UPDATE ordem_servico_pecas osp
SET status_item = 1,
    aplicada = CASE WHEN o.status IN (3, 8) THEN TRUE ELSE osp.aplicada END
FROM ordens_servico o
WHERE osp.ordem_servico_id = o.id
  AND o.status IN (1, 2, 3, 5, 6, 7, 8)
  AND osp.status_item = 0;

-- 4. Concluida (3) antigas -> Entregue (8), com DataEntrega = DataConclusao
UPDATE ordens_servico
SET status = 8,
    data_entrega = COALESCE(data_entrega, data_conclusao, data_emissao)
WHERE status = 3;

-- 5. MeioAutorizacao textual -> TipoContatoAutorizacao (0=Ligacao, 1=WhatsApp, 2=Email, 3=Presencial, 4=Outro)
UPDATE ordens_servico
SET tipo_contato_autorizacao = CASE
    WHEN meio_autorizacao ILIKE '%presencial%' THEN 3
    WHEN meio_autorizacao ILIKE '%whatsapp%' OR meio_autorizacao ILIKE '%whats%' THEN 1
    WHEN meio_autorizacao ILIKE '%telefone%' OR meio_autorizacao ILIKE '%ligac%' OR meio_autorizacao ILIKE '%ligaç%' OR meio_autorizacao ILIKE '%fone%' THEN 0
    WHEN meio_autorizacao ILIKE '%e-mail%' OR meio_autorizacao ILIKE '%email%' THEN 2
    ELSE 4
END
WHERE meio_autorizacao IS NOT NULL
  AND BTRIM(meio_autorizacao) <> ''
  AND tipo_contato_autorizacao IS NULL;

-- 6. ValidadeOrcamentoAte retroativa = DataEmissao + 10 dias
UPDATE ordens_servico
SET validade_orcamento_ate = data_emissao + INTERVAL '10 days'
WHERE validade_orcamento_ate <= '1900-01-01 00:00:00+00'::timestamptz;

-- 7. KmEntrada a partir da Vistoria; senão da Veiculo.Quilometragem
UPDATE ordens_servico o
SET km_entrada = COALESCE(
    (SELECT v.quilometragem_entrada FROM vistorias v WHERE v.ordem_servico_id = o.id AND v.quilometragem_entrada > 0 LIMIT 1),
    (SELECT ve.quilometragem FROM veiculos ve WHERE ve.id = o.veiculo_id),
    0
)
WHERE o.km_entrada = 0;

-- 8. NivelCombustivelEntrada a partir da Vistoria; senão Reserva (1)
UPDATE ordens_servico o
SET nivel_combustivel_entrada = v.nivel_combustivel
FROM vistorias v
WHERE v.ordem_servico_id = o.id
  AND v.nivel_combustivel BETWEEN 1 AND 5;

UPDATE ordens_servico
SET nivel_combustivel_entrada = 1
WHERE nivel_combustivel_entrada NOT BETWEEN 1 AND 5;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ordem_servico_servicos_usuarios_mecanico_id",
                table: "ordem_servico_servicos");

            migrationBuilder.DropForeignKey(
                name: "fk_ordens_servico_usuarios_aprovado_por_usuario_id",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "fk_ordens_servico_usuarios_mecanico_responsavel_id",
                table: "ordens_servico");

            migrationBuilder.DropTable(
                name: "apontamentos_tempo");

            migrationBuilder.DropTable(
                name: "pagamentos");

            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_aprovado_por_usuario_id",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_mecanico_responsavel_id",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_ordem_servico_servicos_mecanico_id",
                table: "ordem_servico_servicos");

            migrationBuilder.DropColumn(
                name: "aprovado_por_usuario_id",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "data_entrega",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "diagnostico",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "garantia_dias",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "km_entrada",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "km_saida",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "mecanico_responsavel_id",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "nivel_combustivel_entrada",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "queixa_cliente",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "tipo_contato_autorizacao",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "validade_orcamento_ate",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "eh_aditivo",
                table: "ordem_servico_servicos");

            migrationBuilder.DropColumn(
                name: "executado",
                table: "ordem_servico_servicos");

            migrationBuilder.DropColumn(
                name: "mecanico_id",
                table: "ordem_servico_servicos");

            migrationBuilder.DropColumn(
                name: "status_item",
                table: "ordem_servico_servicos");

            migrationBuilder.DropColumn(
                name: "aplicada",
                table: "ordem_servico_pecas");

            migrationBuilder.DropColumn(
                name: "eh_aditivo",
                table: "ordem_servico_pecas");

            migrationBuilder.DropColumn(
                name: "origem",
                table: "ordem_servico_pecas");

            migrationBuilder.DropColumn(
                name: "status_item",
                table: "ordem_servico_pecas");

            migrationBuilder.AlterColumn<string>(
                name: "descricao_problema_relatado",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
