using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mechsystem.Migrations
{
    /// <inheritdoc />
    public partial class AplicarSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContatosOS_OrdensServico_OrdemServicoId",
                table: "ContatosOS");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Pecas_PecaId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Usuarios_UsuarioId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoPecas_OrdensServico_OrdemServicoId",
                table: "OrdemServicoPecas");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoPecas_Pecas_PecaId",
                table: "OrdemServicoPecas");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoServicos_OrdensServico_OrdemServicoId",
                table: "OrdemServicoServicos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoServicos_Servicos_ServicoId",
                table: "OrdemServicoServicos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensServico_Veiculos_VeiculoId",
                table: "OrdensServico");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vistorias_OrdensServico_OrdemServicoId",
                table: "Vistorias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vistorias",
                table: "Vistorias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Veiculos",
                table: "Veiculos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servicos",
                table: "Servicos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pecas",
                table: "Pecas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Configuracoes",
                table: "Configuracoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdensServico",
                table: "OrdensServico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdemServicoServicos",
                table: "OrdemServicoServicos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrdemServicoPecas",
                table: "OrdemServicoPecas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimentacoesEstoque",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContatosOS",
                table: "ContatosOS");

            migrationBuilder.RenameTable(
                name: "Vistorias",
                newName: "vistorias");

            migrationBuilder.RenameTable(
                name: "Veiculos",
                newName: "veiculos");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "usuarios");

            migrationBuilder.RenameTable(
                name: "Servicos",
                newName: "servicos");

            migrationBuilder.RenameTable(
                name: "Pecas",
                newName: "pecas");

            migrationBuilder.RenameTable(
                name: "Configuracoes",
                newName: "configuracoes");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "clientes");

            migrationBuilder.RenameTable(
                name: "OrdensServico",
                newName: "ordens_servico");

            migrationBuilder.RenameTable(
                name: "OrdemServicoServicos",
                newName: "ordem_servico_servicos");

            migrationBuilder.RenameTable(
                name: "OrdemServicoPecas",
                newName: "ordem_servico_pecas");

            migrationBuilder.RenameTable(
                name: "MovimentacoesEstoque",
                newName: "movimentacoes_estoque");

            migrationBuilder.RenameTable(
                name: "ContatosOS",
                newName: "contatos_os");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "vistorias",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Observacoes",
                table: "vistorias",
                newName: "observacoes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "vistorias",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TemTriangulo",
                table: "vistorias",
                newName: "tem_triangulo");

            migrationBuilder.RenameColumn(
                name: "TemRadio",
                table: "vistorias",
                newName: "tem_radio");

            migrationBuilder.RenameColumn(
                name: "TemMacaco",
                table: "vistorias",
                newName: "tem_macaco");

            migrationBuilder.RenameColumn(
                name: "TemEstepe",
                table: "vistorias",
                newName: "tem_estepe");

            migrationBuilder.RenameColumn(
                name: "TemChaveRoda",
                table: "vistorias",
                newName: "tem_chave_roda");

            migrationBuilder.RenameColumn(
                name: "QuilometragemEntrada",
                table: "vistorias",
                newName: "quilometragem_entrada");

            migrationBuilder.RenameColumn(
                name: "OrdemServicoId",
                table: "vistorias",
                newName: "ordem_servico_id");

            migrationBuilder.RenameColumn(
                name: "NivelCombustivel",
                table: "vistorias",
                newName: "nivel_combustivel");

            migrationBuilder.RenameColumn(
                name: "DataVistoria",
                table: "vistorias",
                newName: "data_vistoria");

            migrationBuilder.RenameColumn(
                name: "AvariasJson",
                table: "vistorias",
                newName: "avarias_json");

            migrationBuilder.RenameIndex(
                name: "IX_Vistorias_OrdemServicoId",
                table: "vistorias",
                newName: "ix_vistorias_ordem_servico_id");

            migrationBuilder.RenameColumn(
                name: "Quilometragem",
                table: "veiculos",
                newName: "quilometragem");

            migrationBuilder.RenameColumn(
                name: "Placa",
                table: "veiculos",
                newName: "placa");

            migrationBuilder.RenameColumn(
                name: "Modelo",
                table: "veiculos",
                newName: "modelo");

            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "veiculos",
                newName: "marca");

            migrationBuilder.RenameColumn(
                name: "Cor",
                table: "veiculos",
                newName: "cor");

            migrationBuilder.RenameColumn(
                name: "Ano",
                table: "veiculos",
                newName: "ano");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "veiculos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "veiculos",
                newName: "cliente_id");

            migrationBuilder.RenameIndex(
                name: "IX_Veiculos_ClienteId",
                table: "veiculos",
                newName: "ix_veiculos_cliente_id");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "usuarios",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Perfil",
                table: "usuarios",
                newName: "perfil");

            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "usuarios",
                newName: "ativo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "usuarios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "usuarios",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "NomeCompleto",
                table: "usuarios",
                newName: "nome_completo");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "usuarios",
                newName: "data_criacao");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_Username",
                table: "usuarios",
                newName: "ix_usuarios_username");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "servicos",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "servicos",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "servicos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ValorPadrao",
                table: "servicos",
                newName: "valor_padrao");

            migrationBuilder.RenameColumn(
                name: "TempoEstimadoMinutos",
                table: "servicos",
                newName: "tempo_estimado_minutos");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "pecas",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "pecas",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Marca",
                table: "pecas",
                newName: "marca");

            migrationBuilder.RenameColumn(
                name: "Localizacao",
                table: "pecas",
                newName: "localizacao");

            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "pecas",
                newName: "ativo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "pecas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PrecoVenda",
                table: "pecas",
                newName: "preco_venda");

            migrationBuilder.RenameColumn(
                name: "PrecoCusto",
                table: "pecas",
                newName: "preco_custo");

            migrationBuilder.RenameColumn(
                name: "EstoqueMinimo",
                table: "pecas",
                newName: "estoque_minimo");

            migrationBuilder.RenameColumn(
                name: "EstoqueAtual",
                table: "pecas",
                newName: "estoque_atual");

            migrationBuilder.RenameIndex(
                name: "IX_Pecas_Sku",
                table: "pecas",
                newName: "ix_pecas_sku");

            migrationBuilder.RenameColumn(
                name: "Telefone",
                table: "configuracoes",
                newName: "telefone");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "configuracoes",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Cnpj",
                table: "configuracoes",
                newName: "cnpj");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "configuracoes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WhatsApp",
                table: "configuracoes",
                newName: "whats_app");

            migrationBuilder.RenameColumn(
                name: "ValidadeOrcamentoDias",
                table: "configuracoes",
                newName: "validade_orcamento_dias");

            migrationBuilder.RenameColumn(
                name: "TaxaMaoDeObra",
                table: "configuracoes",
                newName: "taxa_mao_de_obra");

            migrationBuilder.RenameColumn(
                name: "SimboloMoeda",
                table: "configuracoes",
                newName: "simbolo_moeda");

            migrationBuilder.RenameColumn(
                name: "ObrigarVistoriaParaOS",
                table: "configuracoes",
                newName: "obrigar_vistoria_para_os");

            migrationBuilder.RenameColumn(
                name: "NomeFantasia",
                table: "configuracoes",
                newName: "nome_fantasia");

            migrationBuilder.RenameColumn(
                name: "MensagemRodape",
                table: "configuracoes",
                newName: "mensagem_rodape");

            migrationBuilder.RenameColumn(
                name: "GarantiaPadraoDias",
                table: "configuracoes",
                newName: "garantia_padrao_dias");

            migrationBuilder.RenameColumn(
                name: "EnderecoCompleto",
                table: "configuracoes",
                newName: "endereco_completo");

            migrationBuilder.RenameColumn(
                name: "Telefone",
                table: "clientes",
                newName: "telefone");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "clientes",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Endereco",
                table: "clientes",
                newName: "endereco");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "clientes",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Cpf",
                table: "clientes",
                newName: "cpf");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "clientes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ordens_servico",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ordens_servico",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VeiculoId",
                table: "ordens_servico",
                newName: "veiculo_id");

            migrationBuilder.RenameColumn(
                name: "ValorPecas",
                table: "ordens_servico",
                newName: "valor_pecas");

            migrationBuilder.RenameColumn(
                name: "ValorMaoDeObra",
                table: "ordens_servico",
                newName: "valor_mao_de_obra");

            migrationBuilder.RenameColumn(
                name: "ValorDesconto",
                table: "ordens_servico",
                newName: "valor_desconto");

            migrationBuilder.RenameColumn(
                name: "TokenAcompanhamento",
                table: "ordens_servico",
                newName: "token_acompanhamento");

            migrationBuilder.RenameColumn(
                name: "ServicoAExecutar",
                table: "ordens_servico",
                newName: "servico_a_executar");

            migrationBuilder.RenameColumn(
                name: "MeioAutorizacao",
                table: "ordens_servico",
                newName: "meio_autorizacao");

            migrationBuilder.RenameColumn(
                name: "DescricaoProblemaRelatado",
                table: "ordens_servico",
                newName: "descricao_problema_relatado");

            migrationBuilder.RenameColumn(
                name: "DataPrevisaoInicio",
                table: "ordens_servico",
                newName: "data_previsao_inicio");

            migrationBuilder.RenameColumn(
                name: "DataPrevisaoEntrega",
                table: "ordens_servico",
                newName: "data_previsao_entrega");

            migrationBuilder.RenameColumn(
                name: "DataEmissao",
                table: "ordens_servico",
                newName: "data_emissao");

            migrationBuilder.RenameColumn(
                name: "DataAutorizacao",
                table: "ordens_servico",
                newName: "data_autorizacao");

            migrationBuilder.RenameColumn(
                name: "AutorizadoPor",
                table: "ordens_servico",
                newName: "autorizado_por");

            migrationBuilder.RenameIndex(
                name: "IX_OrdensServico_VeiculoId",
                table: "ordens_servico",
                newName: "ix_ordens_servico_veiculo_id");

            migrationBuilder.RenameColumn(
                name: "Quantidade",
                table: "ordem_servico_servicos",
                newName: "quantidade");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ordem_servico_servicos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ValorCobrado",
                table: "ordem_servico_servicos",
                newName: "valor_cobrado");

            migrationBuilder.RenameColumn(
                name: "TempoEstimadoMinutosSnapshot",
                table: "ordem_servico_servicos",
                newName: "tempo_estimado_minutos_snapshot");

            migrationBuilder.RenameColumn(
                name: "ServicoId",
                table: "ordem_servico_servicos",
                newName: "servico_id");

            migrationBuilder.RenameColumn(
                name: "PrecoBase",
                table: "ordem_servico_servicos",
                newName: "preco_base");

            migrationBuilder.RenameColumn(
                name: "OrdemServicoId",
                table: "ordem_servico_servicos",
                newName: "ordem_servico_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrdemServicoServicos_ServicoId",
                table: "ordem_servico_servicos",
                newName: "ix_ordem_servico_servicos_servico_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrdemServicoServicos_OrdemServicoId",
                table: "ordem_servico_servicos",
                newName: "ix_ordem_servico_servicos_ordem_servico_id");

            migrationBuilder.RenameColumn(
                name: "Quantidade",
                table: "ordem_servico_pecas",
                newName: "quantidade");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ordem_servico_pecas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ValorCobrado",
                table: "ordem_servico_pecas",
                newName: "valor_cobrado");

            migrationBuilder.RenameColumn(
                name: "PrecoCustoSnapshot",
                table: "ordem_servico_pecas",
                newName: "preco_custo_snapshot");

            migrationBuilder.RenameColumn(
                name: "PrecoBase",
                table: "ordem_servico_pecas",
                newName: "preco_base");

            migrationBuilder.RenameColumn(
                name: "PecaId",
                table: "ordem_servico_pecas",
                newName: "peca_id");

            migrationBuilder.RenameColumn(
                name: "OrdemServicoId",
                table: "ordem_servico_pecas",
                newName: "ordem_servico_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrdemServicoPecas_PecaId",
                table: "ordem_servico_pecas",
                newName: "ix_ordem_servico_pecas_peca_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrdemServicoPecas_OrdemServicoId",
                table: "ordem_servico_pecas",
                newName: "ix_ordem_servico_pecas_ordem_servico_id");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "movimentacoes_estoque",
                newName: "tipo");

            migrationBuilder.RenameColumn(
                name: "Referencia",
                table: "movimentacoes_estoque",
                newName: "referencia");

            migrationBuilder.RenameColumn(
                name: "Quantidade",
                table: "movimentacoes_estoque",
                newName: "quantidade");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "movimentacoes_estoque",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "movimentacoes_estoque",
                newName: "usuario_id");

            migrationBuilder.RenameColumn(
                name: "PecaId",
                table: "movimentacoes_estoque",
                newName: "peca_id");

            migrationBuilder.RenameColumn(
                name: "DataHora",
                table: "movimentacoes_estoque",
                newName: "data_hora");

            migrationBuilder.RenameIndex(
                name: "IX_MovimentacoesEstoque_UsuarioId",
                table: "movimentacoes_estoque",
                newName: "ix_movimentacoes_estoque_usuario_id");

            migrationBuilder.RenameIndex(
                name: "IX_MovimentacoesEstoque_PecaId",
                table: "movimentacoes_estoque",
                newName: "ix_movimentacoes_estoque_peca_id");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "contatos_os",
                newName: "tipo");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "contatos_os",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "contatos_os",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RegistradoPor",
                table: "contatos_os",
                newName: "registrado_por");

            migrationBuilder.RenameColumn(
                name: "OrdemServicoId",
                table: "contatos_os",
                newName: "ordem_servico_id");

            migrationBuilder.RenameColumn(
                name: "DataContato",
                table: "contatos_os",
                newName: "data_contato");

            migrationBuilder.RenameIndex(
                name: "IX_ContatosOS_OrdemServicoId",
                table: "contatos_os",
                newName: "ix_contatos_os_ordem_servico_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_vistoria",
                table: "vistorias",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_criacao",
                table: "usuarios",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_previsao_inicio",
                table: "ordens_servico",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_previsao_entrega",
                table: "ordens_servico",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_emissao",
                table: "ordens_servico",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_autorizacao",
                table: "ordens_servico",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_hora",
                table: "movimentacoes_estoque",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data_contato",
                table: "contatos_os",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "pk_vistorias",
                table: "vistorias",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_veiculos",
                table: "veiculos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_usuarios",
                table: "usuarios",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_servicos",
                table: "servicos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pecas",
                table: "pecas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_configuracoes",
                table: "configuracoes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_clientes",
                table: "clientes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ordens_servico",
                table: "ordens_servico",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ordem_servico_servicos",
                table: "ordem_servico_servicos",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ordem_servico_pecas",
                table: "ordem_servico_pecas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_movimentacoes_estoque",
                table: "movimentacoes_estoque",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_contatos_os",
                table: "contatos_os",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_contatos_os_ordens_servico_ordem_servico_id",
                table: "contatos_os",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_estoque_pecas_peca_id",
                table: "movimentacoes_estoque",
                column: "peca_id",
                principalTable: "pecas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_estoque_usuarios_usuario_id",
                table: "movimentacoes_estoque",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ordem_servico_pecas_ordens_servico_ordem_servico_id",
                table: "ordem_servico_pecas",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ordem_servico_pecas_pecas_peca_id",
                table: "ordem_servico_pecas",
                column: "peca_id",
                principalTable: "pecas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ordem_servico_servicos_ordens_servico_ordem_servico_id",
                table: "ordem_servico_servicos",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ordem_servico_servicos_servicos_servico_id",
                table: "ordem_servico_servicos",
                column: "servico_id",
                principalTable: "servicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ordens_servico_veiculos_veiculo_id",
                table: "ordens_servico",
                column: "veiculo_id",
                principalTable: "veiculos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_veiculos_clientes_cliente_id",
                table: "veiculos",
                column: "cliente_id",
                principalTable: "clientes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vistorias_ordens_servico_ordem_servico_id",
                table: "vistorias",
                column: "ordem_servico_id",
                principalTable: "ordens_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contatos_os_ordens_servico_ordem_servico_id",
                table: "contatos_os");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_estoque_pecas_peca_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_estoque_usuarios_usuario_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropForeignKey(
                name: "fk_ordem_servico_pecas_ordens_servico_ordem_servico_id",
                table: "ordem_servico_pecas");

            migrationBuilder.DropForeignKey(
                name: "fk_ordem_servico_pecas_pecas_peca_id",
                table: "ordem_servico_pecas");

            migrationBuilder.DropForeignKey(
                name: "fk_ordem_servico_servicos_ordens_servico_ordem_servico_id",
                table: "ordem_servico_servicos");

            migrationBuilder.DropForeignKey(
                name: "fk_ordem_servico_servicos_servicos_servico_id",
                table: "ordem_servico_servicos");

            migrationBuilder.DropForeignKey(
                name: "fk_ordens_servico_veiculos_veiculo_id",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "fk_veiculos_clientes_cliente_id",
                table: "veiculos");

            migrationBuilder.DropForeignKey(
                name: "fk_vistorias_ordens_servico_ordem_servico_id",
                table: "vistorias");

            migrationBuilder.DropPrimaryKey(
                name: "pk_vistorias",
                table: "vistorias");

            migrationBuilder.DropPrimaryKey(
                name: "pk_veiculos",
                table: "veiculos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usuarios",
                table: "usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "pk_servicos",
                table: "servicos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pecas",
                table: "pecas");

            migrationBuilder.DropPrimaryKey(
                name: "pk_configuracoes",
                table: "configuracoes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_clientes",
                table: "clientes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ordens_servico",
                table: "ordens_servico");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ordem_servico_servicos",
                table: "ordem_servico_servicos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ordem_servico_pecas",
                table: "ordem_servico_pecas");

            migrationBuilder.DropPrimaryKey(
                name: "pk_movimentacoes_estoque",
                table: "movimentacoes_estoque");

            migrationBuilder.DropPrimaryKey(
                name: "pk_contatos_os",
                table: "contatos_os");

            migrationBuilder.RenameTable(
                name: "vistorias",
                newName: "Vistorias");

            migrationBuilder.RenameTable(
                name: "veiculos",
                newName: "Veiculos");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "servicos",
                newName: "Servicos");

            migrationBuilder.RenameTable(
                name: "pecas",
                newName: "Pecas");

            migrationBuilder.RenameTable(
                name: "configuracoes",
                newName: "Configuracoes");

            migrationBuilder.RenameTable(
                name: "clientes",
                newName: "Clientes");

            migrationBuilder.RenameTable(
                name: "ordens_servico",
                newName: "OrdensServico");

            migrationBuilder.RenameTable(
                name: "ordem_servico_servicos",
                newName: "OrdemServicoServicos");

            migrationBuilder.RenameTable(
                name: "ordem_servico_pecas",
                newName: "OrdemServicoPecas");

            migrationBuilder.RenameTable(
                name: "movimentacoes_estoque",
                newName: "MovimentacoesEstoque");

            migrationBuilder.RenameTable(
                name: "contatos_os",
                newName: "ContatosOS");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Vistorias",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "observacoes",
                table: "Vistorias",
                newName: "Observacoes");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Vistorias",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tem_triangulo",
                table: "Vistorias",
                newName: "TemTriangulo");

            migrationBuilder.RenameColumn(
                name: "tem_radio",
                table: "Vistorias",
                newName: "TemRadio");

            migrationBuilder.RenameColumn(
                name: "tem_macaco",
                table: "Vistorias",
                newName: "TemMacaco");

            migrationBuilder.RenameColumn(
                name: "tem_estepe",
                table: "Vistorias",
                newName: "TemEstepe");

            migrationBuilder.RenameColumn(
                name: "tem_chave_roda",
                table: "Vistorias",
                newName: "TemChaveRoda");

            migrationBuilder.RenameColumn(
                name: "quilometragem_entrada",
                table: "Vistorias",
                newName: "QuilometragemEntrada");

            migrationBuilder.RenameColumn(
                name: "ordem_servico_id",
                table: "Vistorias",
                newName: "OrdemServicoId");

            migrationBuilder.RenameColumn(
                name: "nivel_combustivel",
                table: "Vistorias",
                newName: "NivelCombustivel");

            migrationBuilder.RenameColumn(
                name: "data_vistoria",
                table: "Vistorias",
                newName: "DataVistoria");

            migrationBuilder.RenameColumn(
                name: "avarias_json",
                table: "Vistorias",
                newName: "AvariasJson");

            migrationBuilder.RenameIndex(
                name: "ix_vistorias_ordem_servico_id",
                table: "Vistorias",
                newName: "IX_Vistorias_OrdemServicoId");

            migrationBuilder.RenameColumn(
                name: "quilometragem",
                table: "Veiculos",
                newName: "Quilometragem");

            migrationBuilder.RenameColumn(
                name: "placa",
                table: "Veiculos",
                newName: "Placa");

            migrationBuilder.RenameColumn(
                name: "modelo",
                table: "Veiculos",
                newName: "Modelo");

            migrationBuilder.RenameColumn(
                name: "marca",
                table: "Veiculos",
                newName: "Marca");

            migrationBuilder.RenameColumn(
                name: "cor",
                table: "Veiculos",
                newName: "Cor");

            migrationBuilder.RenameColumn(
                name: "ano",
                table: "Veiculos",
                newName: "Ano");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Veiculos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "cliente_id",
                table: "Veiculos",
                newName: "ClienteId");

            migrationBuilder.RenameIndex(
                name: "ix_veiculos_cliente_id",
                table: "Veiculos",
                newName: "IX_Veiculos_ClienteId");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Usuarios",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "perfil",
                table: "Usuarios",
                newName: "Perfil");

            migrationBuilder.RenameColumn(
                name: "ativo",
                table: "Usuarios",
                newName: "Ativo");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Usuarios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Usuarios",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "nome_completo",
                table: "Usuarios",
                newName: "NomeCompleto");

            migrationBuilder.RenameColumn(
                name: "data_criacao",
                table: "Usuarios",
                newName: "DataCriacao");

            migrationBuilder.RenameIndex(
                name: "ix_usuarios_username",
                table: "Usuarios",
                newName: "IX_Usuarios_Username");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Servicos",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "Servicos",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Servicos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "valor_padrao",
                table: "Servicos",
                newName: "ValorPadrao");

            migrationBuilder.RenameColumn(
                name: "tempo_estimado_minutos",
                table: "Servicos",
                newName: "TempoEstimadoMinutos");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "Pecas",
                newName: "Sku");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Pecas",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "marca",
                table: "Pecas",
                newName: "Marca");

            migrationBuilder.RenameColumn(
                name: "localizacao",
                table: "Pecas",
                newName: "Localizacao");

            migrationBuilder.RenameColumn(
                name: "ativo",
                table: "Pecas",
                newName: "Ativo");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Pecas",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "preco_venda",
                table: "Pecas",
                newName: "PrecoVenda");

            migrationBuilder.RenameColumn(
                name: "preco_custo",
                table: "Pecas",
                newName: "PrecoCusto");

            migrationBuilder.RenameColumn(
                name: "estoque_minimo",
                table: "Pecas",
                newName: "EstoqueMinimo");

            migrationBuilder.RenameColumn(
                name: "estoque_atual",
                table: "Pecas",
                newName: "EstoqueAtual");

            migrationBuilder.RenameIndex(
                name: "ix_pecas_sku",
                table: "Pecas",
                newName: "IX_Pecas_Sku");

            migrationBuilder.RenameColumn(
                name: "telefone",
                table: "Configuracoes",
                newName: "Telefone");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Configuracoes",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "cnpj",
                table: "Configuracoes",
                newName: "Cnpj");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Configuracoes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "whats_app",
                table: "Configuracoes",
                newName: "WhatsApp");

            migrationBuilder.RenameColumn(
                name: "validade_orcamento_dias",
                table: "Configuracoes",
                newName: "ValidadeOrcamentoDias");

            migrationBuilder.RenameColumn(
                name: "taxa_mao_de_obra",
                table: "Configuracoes",
                newName: "TaxaMaoDeObra");

            migrationBuilder.RenameColumn(
                name: "simbolo_moeda",
                table: "Configuracoes",
                newName: "SimboloMoeda");

            migrationBuilder.RenameColumn(
                name: "obrigar_vistoria_para_os",
                table: "Configuracoes",
                newName: "ObrigarVistoriaParaOS");

            migrationBuilder.RenameColumn(
                name: "nome_fantasia",
                table: "Configuracoes",
                newName: "NomeFantasia");

            migrationBuilder.RenameColumn(
                name: "mensagem_rodape",
                table: "Configuracoes",
                newName: "MensagemRodape");

            migrationBuilder.RenameColumn(
                name: "garantia_padrao_dias",
                table: "Configuracoes",
                newName: "GarantiaPadraoDias");

            migrationBuilder.RenameColumn(
                name: "endereco_completo",
                table: "Configuracoes",
                newName: "EnderecoCompleto");

            migrationBuilder.RenameColumn(
                name: "telefone",
                table: "Clientes",
                newName: "Telefone");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Clientes",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "endereco",
                table: "Clientes",
                newName: "Endereco");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Clientes",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "cpf",
                table: "Clientes",
                newName: "Cpf");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Clientes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OrdensServico",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OrdensServico",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "veiculo_id",
                table: "OrdensServico",
                newName: "VeiculoId");

            migrationBuilder.RenameColumn(
                name: "valor_pecas",
                table: "OrdensServico",
                newName: "ValorPecas");

            migrationBuilder.RenameColumn(
                name: "valor_mao_de_obra",
                table: "OrdensServico",
                newName: "ValorMaoDeObra");

            migrationBuilder.RenameColumn(
                name: "valor_desconto",
                table: "OrdensServico",
                newName: "ValorDesconto");

            migrationBuilder.RenameColumn(
                name: "token_acompanhamento",
                table: "OrdensServico",
                newName: "TokenAcompanhamento");

            migrationBuilder.RenameColumn(
                name: "servico_a_executar",
                table: "OrdensServico",
                newName: "ServicoAExecutar");

            migrationBuilder.RenameColumn(
                name: "meio_autorizacao",
                table: "OrdensServico",
                newName: "MeioAutorizacao");

            migrationBuilder.RenameColumn(
                name: "descricao_problema_relatado",
                table: "OrdensServico",
                newName: "DescricaoProblemaRelatado");

            migrationBuilder.RenameColumn(
                name: "data_previsao_inicio",
                table: "OrdensServico",
                newName: "DataPrevisaoInicio");

            migrationBuilder.RenameColumn(
                name: "data_previsao_entrega",
                table: "OrdensServico",
                newName: "DataPrevisaoEntrega");

            migrationBuilder.RenameColumn(
                name: "data_emissao",
                table: "OrdensServico",
                newName: "DataEmissao");

            migrationBuilder.RenameColumn(
                name: "data_autorizacao",
                table: "OrdensServico",
                newName: "DataAutorizacao");

            migrationBuilder.RenameColumn(
                name: "autorizado_por",
                table: "OrdensServico",
                newName: "AutorizadoPor");

            migrationBuilder.RenameIndex(
                name: "ix_ordens_servico_veiculo_id",
                table: "OrdensServico",
                newName: "IX_OrdensServico_VeiculoId");

            migrationBuilder.RenameColumn(
                name: "quantidade",
                table: "OrdemServicoServicos",
                newName: "Quantidade");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OrdemServicoServicos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "valor_cobrado",
                table: "OrdemServicoServicos",
                newName: "ValorCobrado");

            migrationBuilder.RenameColumn(
                name: "tempo_estimado_minutos_snapshot",
                table: "OrdemServicoServicos",
                newName: "TempoEstimadoMinutosSnapshot");

            migrationBuilder.RenameColumn(
                name: "servico_id",
                table: "OrdemServicoServicos",
                newName: "ServicoId");

            migrationBuilder.RenameColumn(
                name: "preco_base",
                table: "OrdemServicoServicos",
                newName: "PrecoBase");

            migrationBuilder.RenameColumn(
                name: "ordem_servico_id",
                table: "OrdemServicoServicos",
                newName: "OrdemServicoId");

            migrationBuilder.RenameIndex(
                name: "ix_ordem_servico_servicos_servico_id",
                table: "OrdemServicoServicos",
                newName: "IX_OrdemServicoServicos_ServicoId");

            migrationBuilder.RenameIndex(
                name: "ix_ordem_servico_servicos_ordem_servico_id",
                table: "OrdemServicoServicos",
                newName: "IX_OrdemServicoServicos_OrdemServicoId");

            migrationBuilder.RenameColumn(
                name: "quantidade",
                table: "OrdemServicoPecas",
                newName: "Quantidade");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OrdemServicoPecas",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "valor_cobrado",
                table: "OrdemServicoPecas",
                newName: "ValorCobrado");

            migrationBuilder.RenameColumn(
                name: "preco_custo_snapshot",
                table: "OrdemServicoPecas",
                newName: "PrecoCustoSnapshot");

            migrationBuilder.RenameColumn(
                name: "preco_base",
                table: "OrdemServicoPecas",
                newName: "PrecoBase");

            migrationBuilder.RenameColumn(
                name: "peca_id",
                table: "OrdemServicoPecas",
                newName: "PecaId");

            migrationBuilder.RenameColumn(
                name: "ordem_servico_id",
                table: "OrdemServicoPecas",
                newName: "OrdemServicoId");

            migrationBuilder.RenameIndex(
                name: "ix_ordem_servico_pecas_peca_id",
                table: "OrdemServicoPecas",
                newName: "IX_OrdemServicoPecas_PecaId");

            migrationBuilder.RenameIndex(
                name: "ix_ordem_servico_pecas_ordem_servico_id",
                table: "OrdemServicoPecas",
                newName: "IX_OrdemServicoPecas_OrdemServicoId");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "MovimentacoesEstoque",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "referencia",
                table: "MovimentacoesEstoque",
                newName: "Referencia");

            migrationBuilder.RenameColumn(
                name: "quantidade",
                table: "MovimentacoesEstoque",
                newName: "Quantidade");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MovimentacoesEstoque",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usuario_id",
                table: "MovimentacoesEstoque",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "peca_id",
                table: "MovimentacoesEstoque",
                newName: "PecaId");

            migrationBuilder.RenameColumn(
                name: "data_hora",
                table: "MovimentacoesEstoque",
                newName: "DataHora");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoes_estoque_usuario_id",
                table: "MovimentacoesEstoque",
                newName: "IX_MovimentacoesEstoque_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoes_estoque_peca_id",
                table: "MovimentacoesEstoque",
                newName: "IX_MovimentacoesEstoque_PecaId");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "ContatosOS",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "ContatosOS",
                newName: "Descricao");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ContatosOS",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "registrado_por",
                table: "ContatosOS",
                newName: "RegistradoPor");

            migrationBuilder.RenameColumn(
                name: "ordem_servico_id",
                table: "ContatosOS",
                newName: "OrdemServicoId");

            migrationBuilder.RenameColumn(
                name: "data_contato",
                table: "ContatosOS",
                newName: "DataContato");

            migrationBuilder.RenameIndex(
                name: "ix_contatos_os_ordem_servico_id",
                table: "ContatosOS",
                newName: "IX_ContatosOS_OrdemServicoId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataVistoria",
                table: "Vistorias",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCriacao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataPrevisaoInicio",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataPrevisaoEntrega",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataEmissao",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataAutorizacao",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataHora",
                table: "MovimentacoesEstoque",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataContato",
                table: "ContatosOS",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vistorias",
                table: "Vistorias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Veiculos",
                table: "Veiculos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servicos",
                table: "Servicos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pecas",
                table: "Pecas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Configuracoes",
                table: "Configuracoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdensServico",
                table: "OrdensServico",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdemServicoServicos",
                table: "OrdemServicoServicos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrdemServicoPecas",
                table: "OrdemServicoPecas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimentacoesEstoque",
                table: "MovimentacoesEstoque",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Pecas_PecaId",
                table: "MovimentacoesEstoque",
                column: "PecaId",
                principalTable: "Pecas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Usuarios_UsuarioId",
                table: "MovimentacoesEstoque",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoPecas_OrdensServico_OrdemServicoId",
                table: "OrdemServicoPecas",
                column: "OrdemServicoId",
                principalTable: "OrdensServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoPecas_Pecas_PecaId",
                table: "OrdemServicoPecas",
                column: "PecaId",
                principalTable: "Pecas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoServicos_OrdensServico_OrdemServicoId",
                table: "OrdemServicoServicos",
                column: "OrdemServicoId",
                principalTable: "OrdensServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoServicos_Servicos_ServicoId",
                table: "OrdemServicoServicos",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensServico_Veiculos_VeiculoId",
                table: "OrdensServico",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vistorias_OrdensServico_OrdemServicoId",
                table: "Vistorias",
                column: "OrdemServicoId",
                principalTable: "OrdensServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
