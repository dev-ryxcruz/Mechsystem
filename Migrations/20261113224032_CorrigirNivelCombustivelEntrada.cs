using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirNivelCombustivelEntrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ordens_servico
                SET nivel_combustivel_entrada = v.nivel_combustivel
                FROM vistorias v
                WHERE v.ordem_servico_id = ordens_servico.id
                  AND v.nivel_combustivel BETWEEN 1 AND 5
                  AND ordens_servico.nivel_combustivel_entrada NOT BETWEEN 1 AND 5;

                UPDATE ordens_servico
                SET nivel_combustivel_entrada = 1
                WHERE nivel_combustivel_entrada NOT BETWEEN 1 AND 5;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
