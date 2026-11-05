using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelamentoAuditoriaOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cancelada_por_usuario_id",
                table: "ordens_servico",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_cancelamento",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_cancelamento",
                table: "ordens_servico",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_cancelada_por_usuario_id",
                table: "ordens_servico",
                column: "cancelada_por_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ordens_servico_usuarios_cancelada_por_usuario_id",
                table: "ordens_servico",
                column: "cancelada_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ordens_servico_usuarios_cancelada_por_usuario_id",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_cancelada_por_usuario_id",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "cancelada_por_usuario_id",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "data_cancelamento",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "motivo_cancelamento",
                table: "ordens_servico");
        }
    }
}
