using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class PadronizarSoftDeleteInativavel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_inativacao",
                table: "veiculos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "inativado_por_usuario_id",
                table: "veiculos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_inativacao",
                table: "veiculos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "servicos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_inativacao",
                table: "servicos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "inativado_por_usuario_id",
                table: "servicos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_inativacao",
                table: "servicos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_inativacao",
                table: "pecas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "inativado_por_usuario_id",
                table: "pecas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_inativacao",
                table: "pecas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_inativacao",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "inativado_por_usuario_id",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_inativacao",
                table: "clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_veiculos_inativado_por_usuario_id",
                table: "veiculos",
                column: "inativado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_servicos_inativado_por_usuario_id",
                table: "servicos",
                column: "inativado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_pecas_inativado_por_usuario_id",
                table: "pecas",
                column: "inativado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_inativado_por_usuario_id",
                table: "clientes",
                column: "inativado_por_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "fk_clientes_usuarios_inativado_por_usuario_id",
                table: "clientes",
                column: "inativado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pecas_usuarios_inativado_por_usuario_id",
                table: "pecas",
                column: "inativado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_servicos_usuarios_inativado_por_usuario_id",
                table: "servicos",
                column: "inativado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_veiculos_usuarios_inativado_por_usuario_id",
                table: "veiculos",
                column: "inativado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_clientes_usuarios_inativado_por_usuario_id",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "fk_pecas_usuarios_inativado_por_usuario_id",
                table: "pecas");

            migrationBuilder.DropForeignKey(
                name: "fk_servicos_usuarios_inativado_por_usuario_id",
                table: "servicos");

            migrationBuilder.DropForeignKey(
                name: "fk_veiculos_usuarios_inativado_por_usuario_id",
                table: "veiculos");

            migrationBuilder.DropIndex(
                name: "ix_veiculos_inativado_por_usuario_id",
                table: "veiculos");

            migrationBuilder.DropIndex(
                name: "ix_servicos_inativado_por_usuario_id",
                table: "servicos");

            migrationBuilder.DropIndex(
                name: "ix_pecas_inativado_por_usuario_id",
                table: "pecas");

            migrationBuilder.DropIndex(
                name: "ix_clientes_inativado_por_usuario_id",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "data_inativacao",
                table: "veiculos");

            migrationBuilder.DropColumn(
                name: "inativado_por_usuario_id",
                table: "veiculos");

            migrationBuilder.DropColumn(
                name: "motivo_inativacao",
                table: "veiculos");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "servicos");

            migrationBuilder.DropColumn(
                name: "data_inativacao",
                table: "servicos");

            migrationBuilder.DropColumn(
                name: "inativado_por_usuario_id",
                table: "servicos");

            migrationBuilder.DropColumn(
                name: "motivo_inativacao",
                table: "servicos");

            migrationBuilder.DropColumn(
                name: "data_inativacao",
                table: "pecas");

            migrationBuilder.DropColumn(
                name: "inativado_por_usuario_id",
                table: "pecas");

            migrationBuilder.DropColumn(
                name: "motivo_inativacao",
                table: "pecas");

            migrationBuilder.DropColumn(
                name: "data_inativacao",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "inativado_por_usuario_id",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "motivo_inativacao",
                table: "clientes");
        }
    }
}
