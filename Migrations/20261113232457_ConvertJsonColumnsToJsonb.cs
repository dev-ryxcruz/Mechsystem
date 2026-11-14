using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class ConvertJsonColumnsToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE vistorias
                SET avarias_json = NULL
                WHERE avarias_json IS NOT NULL AND btrim(avarias_json) = '';

                UPDATE ordens_servico
                SET checklist_conclusao_json = NULL
                WHERE checklist_conclusao_json IS NOT NULL AND btrim(checklist_conclusao_json) = '';

                ALTER TABLE vistorias
                ALTER COLUMN avarias_json TYPE jsonb
                USING avarias_json::jsonb;

                ALTER TABLE ordens_servico
                ALTER COLUMN checklist_conclusao_json TYPE jsonb
                USING checklist_conclusao_json::jsonb;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE vistorias
                ALTER COLUMN avarias_json TYPE text
                USING avarias_json::text;

                ALTER TABLE ordens_servico
                ALTER COLUMN checklist_conclusao_json TYPE text
                USING checklist_conclusao_json::text;
            ");
        }
    }
}
