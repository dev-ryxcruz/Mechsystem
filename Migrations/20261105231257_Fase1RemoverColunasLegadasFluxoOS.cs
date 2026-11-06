using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class Fase1RemoverColunasLegadasFluxoOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quilometragem_entrada",
                table: "vistorias");

            migrationBuilder.DropColumn(
                name: "data_previsao_inicio",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "descricao_problema_relatado",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "inicio_ultima_execucao",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "meio_autorizacao",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "servico_a_executar",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "tempo_gasto_minutos",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "valor_mao_de_obra",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "valor_pecas",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "simbolo_moeda",
                table: "configuracoes");

            migrationBuilder.DropColumn(
                name: "taxa_mao_de_obra",
                table: "configuracoes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "quilometragem_entrada",
                table: "vistorias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_previsao_inicio",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao_problema_relatado",
                table: "ordens_servico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "inicio_ultima_execucao",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "meio_autorizacao",
                table: "ordens_servico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "servico_a_executar",
                table: "ordens_servico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tempo_gasto_minutos",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_mao_de_obra",
                table: "ordens_servico",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_pecas",
                table: "ordens_servico",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "simbolo_moeda",
                table: "configuracoes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "taxa_mao_de_obra",
                table: "configuracoes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
