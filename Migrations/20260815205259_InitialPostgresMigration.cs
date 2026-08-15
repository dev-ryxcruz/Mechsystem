using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Telefone = table.Column<string>(type: "text", nullable: true),
                    Endereco = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeFantasia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WhatsApp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EnderecoCompleto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MensagemRodape = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidadeOrcamentoDias = table.Column<int>(type: "integer", nullable: false),
                    GarantiaPadraoDias = table.Column<int>(type: "integer", nullable: false),
                    ObrigarVistoriaParaOS = table.Column<bool>(type: "boolean", nullable: false),
                    SimboloMoeda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TaxaMaoDeObra = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pecas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrecoCusto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstoqueAtual = table.Column<int>(type: "integer", nullable: false),
                    EstoqueMinimo = table.Column<int>(type: "integer", nullable: false),
                    Localizacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pecas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValorPadrao = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TempoEstimadoMinutos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    NomeCompleto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Perfil = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: true),
                    Modelo = table.Column<string>(type: "text", nullable: true),
                    Cor = table.Column<string>(type: "text", nullable: true),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Quilometragem = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Veiculos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Pecas_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Pecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdensServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VeiculoId = table.Column<int>(type: "integer", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPrevisaoInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataPrevisaoEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorMaoDeObra = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorPecas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DescricaoProblemaRelatado = table.Column<string>(type: "text", nullable: false),
                    ServicoAExecutar = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AutorizadoPor = table.Column<string>(type: "text", nullable: true),
                    MeioAutorizacao = table.Column<string>(type: "text", nullable: true),
                    DataAutorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenAcompanhamento = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensServico_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContatosOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    DataContato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    RegistradoPor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContatosOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContatosOS_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdemServicoPecas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorCobrado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecoCustoSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoPecas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServicoPecas_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdemServicoPecas_Pecas_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Pecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdemServicoServicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorCobrado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TempoEstimadoMinutosSnapshot = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoServicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServicoServicos_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdemServicoServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vistorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemServicoId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NivelCombustivel = table.Column<int>(type: "integer", nullable: false),
                    QuilometragemEntrada = table.Column<int>(type: "integer", nullable: false),
                    TemEstepe = table.Column<bool>(type: "boolean", nullable: false),
                    TemMacaco = table.Column<bool>(type: "boolean", nullable: false),
                    TemRadio = table.Column<bool>(type: "boolean", nullable: false),
                    TemTriangulo = table.Column<bool>(type: "boolean", nullable: false),
                    TemChaveRoda = table.Column<bool>(type: "boolean", nullable: false),
                    AvariasJson = table.Column<string>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataVistoria = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Ativo", "DataCriacao", "NomeCompleto", "PasswordHash", "Perfil", "Username" },
                values: new object[] { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador", "$2a$11$uegyC1YafmGAkp2uMJmfje/iz6TbLTTiqJgps2rWvCNQFzymrQTza", 2, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_ContatosOS_OrdemServicoId",
                table: "ContatosOS",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_PecaId",
                table: "MovimentacoesEstoque",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_UsuarioId",
                table: "MovimentacoesEstoque",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoPecas_OrdemServicoId",
                table: "OrdemServicoPecas",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoPecas_PecaId",
                table: "OrdemServicoPecas",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoServicos_OrdemServicoId",
                table: "OrdemServicoServicos",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoServicos_ServicoId",
                table: "OrdemServicoServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_VeiculoId",
                table: "OrdensServico",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pecas_Sku",
                table: "Pecas",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Username",
                table: "Usuarios",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_ClienteId",
                table: "Veiculos",
                column: "ClienteId");

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
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "ContatosOS");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "OrdemServicoPecas");

            migrationBuilder.DropTable(
                name: "OrdemServicoServicos");

            migrationBuilder.DropTable(
                name: "Vistorias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Pecas");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "OrdensServico");

            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
