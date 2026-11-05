using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDataConclusaoToOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_conclusao",
                table: "ordens_servico",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE ordens_servico SET data_conclusao = COALESCE(data_autorizacao, data_emissao) WHERE status = 3 AND data_conclusao IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_conclusao",
                table: "ordens_servico");
        }
    }
}
