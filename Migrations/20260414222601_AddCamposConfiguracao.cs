using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposConfiguracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Configuracoes",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensagemRodape",
                table: "Configuracoes",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SimboloMoeda",
                table: "Configuracoes",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaMaoDeObra",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Configuracoes",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "MensagemRodape",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "SimboloMoeda",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "TaxaMaoDeObra",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Configuracoes");
        }
    }
}
