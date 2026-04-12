using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVistoriaEntrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vistorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    NivelCombustivel = table.Column<int>(type: "INTEGER", nullable: false),
                    QuilometragemEntrada = table.Column<int>(type: "INTEGER", nullable: false),
                    TemEstepe = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemMacaco = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemRadio = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemTriangulo = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemChaveRoda = table.Column<bool>(type: "INTEGER", nullable: false),
                    AvariasJson = table.Column<string>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    DataVistoria = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vistorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vistorias_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vistorias_OrdemServicoId",
                table: "Vistorias",
                column: "OrdemServicoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vistorias");
        }
    }
}
