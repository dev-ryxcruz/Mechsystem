using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVeiculoAtivoSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "veiculos",
                type: "boolean",
                nullable: false,
                // Veículos existentes devem continuar ativos após a migration
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ativo",
                table: "veiculos");
        }
    }
}
