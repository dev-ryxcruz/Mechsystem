using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddComunicacaoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenAcompanhamento",
                table: "OrdensServico",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContatoOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataContato = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    RegistradoPor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContatoOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContatoOS_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContatoOS_OrdemServicoId",
                table: "ContatoOS",
                column: "OrdemServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContatoOS");

            migrationBuilder.DropColumn(
                name: "TokenAcompanhamento",
                table: "OrdensServico");
        }
    }
}
