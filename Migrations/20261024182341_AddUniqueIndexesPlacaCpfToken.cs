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
                "UPDATE clientes c SET cpf = left(c.cpf, 10) || lpad((c.id % 10000)::text, 4, '0') WHERE EXISTS (SELECT 1 FROM clientes c2 WHERE c2.cpf = c.cpf AND c2.id < c.id);");
            migrationBuilder.Sql(
                "UPDATE veiculos SET placa = upper(replace(trim(placa), '-', '')) WHERE placa IS NOT NULL;");
            migrationBuilder.Sql(
                "UPDATE veiculos v SET placa = left(v.placa, 6) || lpad((v.id % 10000)::text, 4, '0') WHERE EXISTS (SELECT 1 FROM veiculos v2 WHERE v2.placa = v.placa AND v2.id < v.id);");
            migrationBuilder.Sql(
                "UPDATE ordens_servico SET token_acompanhamento = md5(random()::text || clock_timestamp()::text || id::text) WHERE token_acompanhamento IS NULL OR token_acompanhamento = '';");
            migrationBuilder.Sql(
                "UPDATE ordens_servico o SET token_acompanhamento = md5(random()::text || clock_timestamp()::text || o.id::text) WHERE EXISTS (SELECT 1 FROM ordens_servico o2 WHERE o2.token_acompanhamento = o.token_acompanhamento AND o2.id < o.id);");

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
