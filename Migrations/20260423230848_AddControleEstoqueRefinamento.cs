using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddControleEstoqueRefinamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrecoUnitarioVenda",
                table: "OrdemServicoPecas",
                newName: "ValorCobrado");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoBase",
                table: "OrdemServicoPecas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoBase",
                table: "OrdemServicoPecas");

            migrationBuilder.RenameColumn(
                name: "ValorCobrado",
                table: "OrdemServicoPecas",
                newName: "PrecoUnitarioVenda");
        }
    }
}
