using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddComunicacaoClienteFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContatoOS_OrdensServico_OrdemServicoId",
                table: "ContatoOS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContatoOS",
                table: "ContatoOS");

            migrationBuilder.RenameTable(
                name: "ContatoOS",
                newName: "ContatosOS");

            migrationBuilder.RenameIndex(
                name: "IX_ContatoOS_OrdemServicoId",
                table: "ContatosOS",
                newName: "IX_ContatosOS_OrdemServicoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContatosOS",
                table: "ContatosOS",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContatosOS_OrdensServico_OrdemServicoId",
                table: "ContatosOS",
                column: "OrdemServicoId",
                principalTable: "OrdensServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContatosOS_OrdensServico_OrdemServicoId",
                table: "ContatosOS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContatosOS",
                table: "ContatosOS");

            migrationBuilder.RenameTable(
                name: "ContatosOS",
                newName: "ContatoOS");

            migrationBuilder.RenameIndex(
                name: "IX_ContatosOS_OrdemServicoId",
                table: "ContatoOS",
                newName: "IX_ContatoOS_OrdemServicoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContatoOS",
                table: "ContatoOS",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContatoOS_OrdensServico_OrdemServicoId",
                table: "ContatoOS",
                column: "OrdemServicoId",
                principalTable: "OrdensServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
