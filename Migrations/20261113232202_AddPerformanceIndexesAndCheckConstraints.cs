using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexesAndCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ordem_servico_pecas_peca_id",
                table: "ordem_servico_pecas");

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_status_data_conclusao",
                table: "ordens_servico",
                columns: new[] { "status", "data_conclusao" });

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_status_data_emissao",
                table: "ordens_servico",
                columns: new[] { "status", "data_emissao" });

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_status_mecanico_responsavel_id",
                table: "ordens_servico",
                columns: new[] { "status", "mecanico_responsavel_id" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_ordens_servico_nivel_combustivel",
                table: "ordens_servico",
                sql: "nivel_combustivel_entrada BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "ck_ordens_servico_valor_desconto",
                table: "ordens_servico",
                sql: "valor_desconto >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_pecas_peca_id_ordem_servico_id",
                table: "ordem_servico_pecas",
                columns: new[] { "peca_id", "ordem_servico_id" },
                filter: "origem = 0 AND status_item = 1 AND aplicada = false");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_data_hora_tipo",
                table: "movimentacoes_estoque",
                columns: new[] { "data_hora", "tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_status_data_conclusao",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_status_data_emissao",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_status_mecanico_responsavel_id",
                table: "ordens_servico");

            migrationBuilder.DropCheckConstraint(
                name: "ck_ordens_servico_nivel_combustivel",
                table: "ordens_servico");

            migrationBuilder.DropCheckConstraint(
                name: "ck_ordens_servico_valor_desconto",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_ordem_servico_pecas_peca_id_ordem_servico_id",
                table: "ordem_servico_pecas");

            migrationBuilder.DropIndex(
                name: "ix_movimentacoes_estoque_data_hora_tipo",
                table: "movimentacoes_estoque");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_pecas_peca_id",
                table: "ordem_servico_pecas",
                column: "peca_id");
        }
    }
}
