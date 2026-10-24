using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexesPlacaCpfToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE clientes SET cpf = regexp_replace(cpf, '[^0-9]', '', 'g') WHERE cpf IS NOT NULL;");
            migrationBuilder.Sql(
                "UPDATE veiculos SET placa = upper(replace(trim(placa), '-', '')) WHERE placa IS NOT NULL;");
            migrationBuilder.Sql(
                "UPDATE ordens_servico SET token_acompanhamento = md5(random()::text || clock_timestamp()::text || id::text) WHERE token_acompanhamento IS NULL OR token_acompanhamento = '';");

            migrationBuilder.CreateIndex(
                name: "ix_veiculos_placa",
                table: "veiculos",
                column: "placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ordens_servico_token_acompanhamento",
                table: "ordens_servico",
                column: "token_acompanhamento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_cpf",
                table: "clientes",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_veiculos_placa",
                table: "veiculos");

            migrationBuilder.DropIndex(
                name: "ix_ordens_servico_token_acompanhamento",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "ix_clientes_cpf",
                table: "clientes");
        }
    }
}
