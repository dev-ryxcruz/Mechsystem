using System.Text.Json;
using mechsystem.Data;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services;

public class OrdemServicoService
{
    private readonly AppDbContext _db;
    private readonly EstoqueService _estoqueService;

    private static readonly Dictionary<OrdemServicoStatus, HashSet<OrdemServicoStatus>> TransicoesPermitidas = new()
    {
        [OrdemServicoStatus.Orcamento] = new()
        {
            OrdemServicoStatus.Aprovada,
            OrdemServicoStatus.Recusada,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.Aprovada] = new()
        {
            OrdemServicoStatus.EmAndamento,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.EmAndamento] = new()
        {
            OrdemServicoStatus.Pausada,
            OrdemServicoStatus.AguardandoPecas,
            OrdemServicoStatus.AguardandoCliente,
            OrdemServicoStatus.Concluida,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.Pausada] = new()
        {
            OrdemServicoStatus.EmAndamento,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.AguardandoPecas] = new()
        {
            OrdemServicoStatus.EmAndamento,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.AguardandoCliente] = new()
        {
            OrdemServicoStatus.EmAndamento,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.Concluida] = new()
        {
            OrdemServicoStatus.Entregue
        },
        [OrdemServicoStatus.Entregue] = new(),
        [OrdemServicoStatus.Recusada] = new(),
        [OrdemServicoStatus.Cancelada] = new()
    };

    public OrdemServicoService(AppDbContext db, EstoqueService estoqueService)
    {
        _db = db;
        _estoqueService = estoqueService;
    }

    public async Task<(List<OrdemServico> Itens, int TotalCount)> ListarPaginadoAsync(string? termoBusca, int pagina, int tamanhoPagina, OrdemServicoStatus? status = null)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina <= 0) tamanhoPagina = 10;

        var query = _db.OrdensServico
            .Include(o => o.Veiculo)
                .ThenInclude(v => v!.Cliente)
            .Include(o => o.Vistoria)
            .Include(o => o.PecasUtilizadas)
            .Include(o => o.ServicosAExecutarList)
            .Include(o => o.MecanicoResponsavel)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim().ToLower();
            var placaNorm = termoBusca.Replace("-", "").Trim().ToUpperInvariant();
            var temId = int.TryParse(termoBusca.Trim().TrimStart('#', '0'), out var idBusca);

            query = query.Where(o =>
                (temId && o.Id == idBusca) ||
                (!string.IsNullOrEmpty(placaNorm) && o.Veiculo != null && o.Veiculo.Placa != null && o.Veiculo.Placa.Contains(placaNorm)) ||
                (o.Veiculo != null && o.Veiculo.Modelo != null && o.Veiculo.Modelo.ToLower().Contains(termo)) ||
                (o.Veiculo != null && o.Veiculo.Cliente != null && o.Veiculo.Cliente.Nome != null && o.Veiculo.Cliente.Nome.ToLower().Contains(termo)));
        }

        var totalCount = await query.CountAsync();
        var itens = await query
            .OrderByDescending(o => o.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, totalCount);
    }

    public static bool PodeTransicionar(OrdemServicoStatus origem, OrdemServicoStatus destino)
    {
        return TransicoesPermitidas.TryGetValue(origem, out var destinos) && destinos.Contains(destino);
    }

    public static void ValidarTransicao(OrdemServicoStatus origem, OrdemServicoStatus destino)
    {
        if (!PodeTransicionar(origem, destino))
        {
            throw new InvalidOperationException(
                $"Transição de status inválida: não é permitido alterar a O.S. de '{origem}' para '{destino}'.");
        }
    }

    private static bool EhPerfilValido(PerfilUsuario? perfil) =>
        perfil == PerfilUsuario.Administrador
        || perfil == PerfilUsuario.Atendimento
        || perfil == PerfilUsuario.Mecanico;

    /// <summary>
    /// Calcula a previsão de entrega sugerida = hoje + ceil(TempoTotalEstimadoMinutos / 480) dias úteis, mínimo 1.
    /// </summary>
    public static DateTime CalcularPrevisaoEntregaSugerida(int tempoTotalEstimadoMinutos, DateTime? dataBase = null)
    {
        var inicio = (dataBase ?? DateTimeExtensions.TodayBrasilia()).Date;
        var diasUteis = (int)Math.Ceiling(tempoTotalEstimadoMinutos / 480.0);
        if (diasUteis < 1) diasUteis = 1;

        var dataAtual = inicio;
        var diasAdicionados = 0;

        while (diasAdicionados < diasUteis)
        {
            dataAtual = dataAtual.AddDays(1);
            if (dataAtual.DayOfWeek != DayOfWeek.Saturday && dataAtual.DayOfWeek != DayOfWeek.Sunday)
            {
                diasAdicionados++;
            }
        }

        return dataAtual;
    }

    // ==========================================
    // 0. CRIAÇÃO DE ORÇAMENTO / O.S.
    // ==========================================

    public async Task<int> CriarOrcamentoAsync(
        OrdemServico os,
        int usuarioId = 0,
        string? nomeUsuario = null)
    {
        if (os == null)
        {
            throw new ArgumentNullException(nameof(os));
        }

        if (os.VeiculoId <= 0)
        {
            throw new InvalidOperationException("O veículo é obrigatório.");
        }

        var veiculo = await _db.Veiculos.FindAsync(os.VeiculoId)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        if (os.KmEntrada <= 0)
        {
            throw new InvalidOperationException("O KM de entrada deve ser maior que zero.");
        }

        if ((int)os.NivelCombustivelEntrada < 1 || (int)os.NivelCombustivelEntrada > 5)
        {
            throw new InvalidOperationException("O nível de combustível de entrada é obrigatório.");
        }

        if (os.KmEntrada < veiculo.Quilometragem)
        {
            var operador = !string.IsNullOrWhiteSpace(nomeUsuario)
                ? nomeUsuario
                : await ObterNomeOperadorAsync(usuarioId);

            os.Contatos.Add(new ContatoOS
            {
                Tipo = TipoContato.Outro,
                Descricao = $"Observação na entrada: KM de entrada ({os.KmEntrada:N0}) menor que a quilometragem registrada no veículo ({veiculo.Quilometragem:N0} km).",
                RegistradoPor = operador,
                DataContato = DateTime.UtcNow
            });
        }

        _db.OrdensServico.Add(os);
        await _db.SaveChangesAsync();
        return os.Id;
    }

    // ==========================================
    // 1. APROVAÇÃO DE ORÇAMENTO & RESERVA
    // ==========================================

    public Task AprovarOSAsync(
        int osId,
        string? autorizadoPor,
        string? meioAutorizacao,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var tipoContato = ConverterMeioAutorizacao(meioAutorizacao);
        return AprovarOSAsync(osId, null, null, autorizadoPor, tipoContato, usuarioId, perfilUsuario);
    }

    public async Task AprovarOSAsync(
        int osId,
        IEnumerable<int>? servicosAprovadosIds,
        IEnumerable<int>? pecasAprovadasIds,
        string? autorizadoPor,
        TipoContato? tipoContato,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para aprovar ordens de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.Vistoria)
                .Include(o => o.PecasUtilizadas)
                .Include(o => o.ServicosAExecutarList)
                .Include(o => o.Contatos)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status != OrdemServicoStatus.Orcamento)
            {
                throw new InvalidOperationException(
                    $"Apenas O.S. em Orçamento podem ser aprovadas (status atual: '{os.Status}').");
            }

            ValidarTransicao(os.Status, OrdemServicoStatus.Aprovada);

            // Bloquear se ValidadeOrcamentoAte < hoje (horário de Brasília)
            var hoje = DateTimeExtensions.TodayBrasilia();
            if (os.ValidadeOrcamentoAte.Date < hoje)
            {
                throw new InvalidOperationException(
                    $"Orçamento vencido em {os.ValidadeOrcamentoAte.ToBrasiliaTime():dd/MM/yyyy}. Um Administrador deve renovar a validade do orçamento antes da aprovação.");
            }

            // Exigir KmEntrada > 0 e combustível 1..5
            if (os.KmEntrada <= 0)
            {
                throw new InvalidOperationException("O KM de entrada deve ser maior que zero.");
            }
            if ((int)os.NivelCombustivelEntrada < 1 || (int)os.NivelCombustivelEntrada > 5)
            {
                throw new InvalidOperationException("O nível de combustível de entrada é obrigatório.");
            }

            // Exigir vistoria completa se configurado
            var config = await _db.Configuracoes.FirstOrDefaultAsync();
            if (config != null && config.ObrigarVistoriaParaOS && (os.Vistoria == null || os.Vistoria.Status != VistoriaStatus.Concluida))
            {
                throw new InvalidOperationException(
                    "Para aprovar este orçamento, as configurações da Oficina exigem a realização e conclusão da Vistoria de Entrada.");
            }

            // Aplica aprovação individual dos itens
            var servicosAprovadosSet = servicosAprovadosIds?.ToHashSet();
            var pecasAprovadasSet = pecasAprovadasIds?.ToHashSet();

            foreach (var servico in os.ServicosAExecutarList)
            {
                if (servicosAprovadosSet == null || servicosAprovadosSet.Contains(servico.Id))
                {
                    servico.StatusItem = StatusItem.Aprovado;
                }
                else
                {
                    servico.StatusItem = StatusItem.Recusado;
                }
            }

            foreach (var peca in os.PecasUtilizadas)
            {
                if (pecasAprovadasSet == null || pecasAprovadasSet.Contains(peca.Id))
                {
                    peca.StatusItem = StatusItem.Aprovado;
                }
                else
                {
                    peca.StatusItem = StatusItem.Recusado;
                }
            }

            var totalItensAprovados = os.ServicosAExecutarList.Count(s => s.StatusItem == StatusItem.Aprovado)
                + os.PecasUtilizadas.Count(p => p.StatusItem == StatusItem.Aprovado);

            if (totalItensAprovados == 0)
            {
                throw new InvalidOperationException(
                    "Ao menos um item deve ser aprovado. Se nenhum item foi aceito pelo cliente, utilize a opção 'Recusar Orçamento'.");
            }

            // Valida e RESERVA estoque para peças aprovadas de origem Estoque (não baixa fisicamente)
            var pecasEstoqueAprovadasIds = os.PecasUtilizadas
                .Where(p => p.StatusItem == StatusItem.Aprovado && p.Origem == OrigemPeca.Estoque)
                .Select(p => p.Id)
                .ToList();

            if (pecasEstoqueAprovadasIds.Any())
            {
                await _estoqueService.ValidarDisponibilidadePecasAprovadasAsync(os.Id, pecasEstoqueAprovadasIds);
            }

            var canalAutorizacao = tipoContato ?? TipoContato.Presencial;
            var autorizador = string.IsNullOrWhiteSpace(autorizadoPor) ? "Cliente" : autorizadoPor.Trim();
            var nomeOperador = await ObterNomeOperadorAsync(usuarioId);

            os.Status = OrdemServicoStatus.Aprovada;
            os.DataAutorizacao = DateTime.UtcNow;
            os.AutorizadoPor = autorizador;
            os.AprovadoPorUsuarioId = usuarioId > 0 ? usuarioId : null;
            os.TipoContatoAutorizacao = canalAutorizacao;

            var contato = new ContatoOS
            {
                OrdemServicoId = os.Id,
                Tipo = canalAutorizacao,
                Descricao = $"Orçamento aprovado por {autorizador} via {canalAutorizacao}. Total contratado: {os.ValorTotal:C}. Registrado por {nomeOperador}.",
                RegistradoPor = nomeOperador,
                DataContato = DateTime.UtcNow
            };
            _db.ContatosOS.Add(contato);

            await _db.SaveChangesAsync();
        });
    }

    public async Task RenovarValidadeOrcamentoAsync(
        int osId,
        DateTime novaValidade,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null,
        string? nomeUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (perfil != PerfilUsuario.Administrador)
        {
            throw new UnauthorizedAccessException("Apenas Administradores podem renovar a validade do orçamento.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.Contatos)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status != OrdemServicoStatus.Orcamento)
        {
            throw new InvalidOperationException("Apenas ordens de serviço em Orçamento podem ter a validade renovada.");
        }

        var hoje = DateTimeExtensions.TodayBrasilia();
        if (novaValidade.Date < hoje)
        {
            throw new InvalidOperationException("A nova data de validade não pode ser anterior à data de hoje.");
        }

        var validadeAntiga = os.ValidadeOrcamentoAte.ToBrasiliaTime().ToString("dd/MM/yyyy");
        var validadeNova = novaValidade.ToString("dd/MM/yyyy");
        os.ValidadeOrcamentoAte = DateTime.SpecifyKind(novaValidade.Date, DateTimeKind.Utc);

        var operador = nomeUsuario ?? await ObterNomeOperadorAsync(usuarioId);

        var contato = new ContatoOS
        {
            OrdemServicoId = os.Id,
            Tipo = TipoContato.Outro,
            Descricao = $"Validade do orçamento renovada de {validadeAntiga} para {validadeNova} por {operador}",
            RegistradoPor = operador,
            DataContato = DateTime.UtcNow
        };
        _db.ContatosOS.Add(contato);

        await _db.SaveChangesAsync();
    }

    // ==========================================
    // 2. RECUSA DE ORÇAMENTO
    // ==========================================

    public async Task RecusarOrcamentoAsync(
        int osId,
        string motivo,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null,
        string? nomeUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para recusar orçamentos.");
        }

        var motivoLimpo = motivo?.Trim() ?? string.Empty;
        if (motivoLimpo.Length < 5)
        {
            throw new InvalidOperationException("O motivo da recusa é obrigatório e deve ter pelo menos 5 caracteres.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.Contatos)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status != OrdemServicoStatus.Orcamento)
        {
            throw new InvalidOperationException($"Apenas O.S. em Orçamento podem ser recusadas (status atual: '{os.Status}').");
        }

        ValidarTransicao(os.Status, OrdemServicoStatus.Recusada);

        os.Status = OrdemServicoStatus.Recusada;
        os.MotivoCancelamento = motivoLimpo;

        var operador = nomeUsuario ?? await ObterNomeOperadorAsync(usuarioId);

        var contato = new ContatoOS
        {
            OrdemServicoId = os.Id,
            Tipo = TipoContato.Outro,
            Descricao = $"Orçamento recusado pelo cliente. Motivo: {motivoLimpo} (Registrado por {operador})",
            RegistradoPor = operador,
            DataContato = DateTime.UtcNow
        };
        _db.ContatosOS.Add(contato);

        await _db.SaveChangesAsync();
    }

    // ==========================================
    // 3. ATRIBUIÇÃO DE MECÂNICO
    // ==========================================

    public async Task AtribuirMecanicoAsync(
        int osId,
        int mecanicoId,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null,
        string? nomeUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para atribuir mecânico.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.Contatos)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status == OrdemServicoStatus.Concluida ||
            os.Status == OrdemServicoStatus.Entregue ||
            os.Status == OrdemServicoStatus.Cancelada ||
            os.Status == OrdemServicoStatus.Recusada)
        {
            throw new InvalidOperationException($"Não é permitido alterar o mecânico responsável em uma O.S. com status '{os.Status}'.");
        }

        var mecanico = await _db.Usuarios.FindAsync(mecanicoId)
            ?? throw new InvalidOperationException("Mecânico não encontrado.");

        if (mecanico.Perfil != PerfilUsuario.Mecanico && mecanico.Perfil != PerfilUsuario.Administrador)
        {
            throw new InvalidOperationException("O usuário selecionado não possui perfil técnico para assumir a O.S.");
        }

        if (perfil == PerfilUsuario.Mecanico)
        {
            if (os.MecanicoResponsavelId.HasValue && os.MecanicoResponsavelId.Value > 0)
            {
                throw new UnauthorizedAccessException("Esta O.S. já possui um mecânico responsável atribuído. Apenas Atendimento ou Administrador podem reatribuir.");
            }

            if (mecanicoId != usuarioId)
            {
                throw new UnauthorizedAccessException("Um mecânico só pode assumir a O.S. para si próprio.");
            }
        }

        os.MecanicoResponsavelId = mecanicoId;

        var operador = nomeUsuario ?? await ObterNomeOperadorAsync(usuarioId);

        var contato = new ContatoOS
        {
            OrdemServicoId = os.Id,
            Tipo = TipoContato.Outro,
            Descricao = $"Mecânico responsável definido como '{mecanico.NomeCompleto}' por {operador}",
            RegistradoPor = operador,
            DataContato = DateTime.UtcNow
        };
        _db.ContatosOS.Add(contato);

        await _db.SaveChangesAsync();
    }

    // ==========================================
    // 4. EXECUÇÃO & APONTAMENTOS DE TEMPO
    // ==========================================

    public async Task<int> IniciarApontamentoAsync(
        int osId,
        int? servicoItemId,
        int mecanicoId,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para iniciar apontamento de tempo.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.ApontamentosTempo)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status == OrdemServicoStatus.Orcamento)
        {
            throw new InvalidOperationException("A O.S. ainda está em Orçamento e precisa ser aprovada antes de iniciar a execução.");
        }

        if (os.Status == OrdemServicoStatus.Recusada ||
            os.Status == OrdemServicoStatus.Cancelada ||
            os.Status == OrdemServicoStatus.Concluida ||
            os.Status == OrdemServicoStatus.Entregue)
        {
            throw new InvalidOperationException($"Não é permitido iniciar apontamentos de tempo em uma O.S. com status '{os.Status}'.");
        }

        var mecId = mecanicoId > 0 ? mecanicoId : (usuarioId > 0 ? usuarioId : os.MecanicoResponsavelId ?? 0);
        if (mecId <= 0)
        {
            throw new InvalidOperationException("É necessário identificar o usuário/mecânico para iniciar o apontamento de tempo.");
        }

        // Impedir dois apontamentos abertos para o mesmo mecânico
        var temAberto = await _db.ApontamentosTempo.AnyAsync(a => a.MecanicoId == mecId && a.Fim == null);
        if (temAberto)
        {
            throw new InvalidOperationException("O mecânico já possui um apontamento de tempo em andamento. Pause o apontamento anterior antes de iniciar um novo.");
        }

        // Primeira execução move Aprovada -> EmAndamento
        if (os.Status == OrdemServicoStatus.Aprovada)
        {
            ValidarTransicao(os.Status, OrdemServicoStatus.EmAndamento);
            os.Status = OrdemServicoStatus.EmAndamento;
        }
        else if (os.Status == OrdemServicoStatus.Pausada || os.Status == OrdemServicoStatus.AguardandoPecas || os.Status == OrdemServicoStatus.AguardandoCliente)
        {
            ValidarTransicao(os.Status, OrdemServicoStatus.EmAndamento);
            os.Status = OrdemServicoStatus.EmAndamento;
        }

        var apontamento = new ApontamentoTempo
        {
            OrdemServicoId = os.Id,
            OrdemServicoServicoId = servicoItemId,
            MecanicoId = mecId,
            Inicio = DateTime.UtcNow,
            Fim = null
        };

        _db.ApontamentosTempo.Add(apontamento);

        await _db.SaveChangesAsync();
        return apontamento.Id;
    }

    public async Task PausarApontamentoAsync(
        int osId,
        int? apontamentoId,
        int mecanicoId,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para pausar apontamento de tempo.");
        }

        var mecId = mecanicoId > 0 ? mecanicoId : usuarioId;

        var query = _db.ApontamentosTempo.Where(a => a.OrdemServicoId == osId && a.Fim == null);
        if (apontamentoId.HasValue && apontamentoId.Value > 0)
        {
            query = query.Where(a => a.Id == apontamentoId.Value);
        }
        else if (mecId > 0)
        {
            query = query.Where(a => a.MecanicoId == mecId);
        }

        var apontamento = await query.FirstOrDefaultAsync();
        if (apontamento != null)
        {
            apontamento.Fim = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task IniciarOuRetomarServicoAsync(int osId, PerfilUsuario? perfilUsuario = null, int usuarioId = 0)
    {
        await IniciarApontamentoAsync(osId, null, usuarioId, usuarioId, perfilUsuario);
    }

    public async Task PausarServicoAsync(int osId, PerfilUsuario? perfilUsuario = null, int usuarioId = 0)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para pausar a ordem de serviço.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.ApontamentosTempo)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.Pausada);

        FecharApontamentosAbertos(os);
        os.Status = OrdemServicoStatus.Pausada;
        await _db.SaveChangesAsync();
    }

    public async Task AguardarPecasAsync(int osId, PerfilUsuario? perfilUsuario = null, int usuarioId = 0)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para alterar o status da ordem de serviço.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.ApontamentosTempo)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.AguardandoPecas);

        FecharApontamentosAbertos(os);
        os.Status = OrdemServicoStatus.AguardandoPecas;
        await _db.SaveChangesAsync();
    }

    // ==========================================
    // 5. BAIXA DE PEÇA APLICADA & MARCAÇÃO DE SERVIÇO
    // ==========================================

    public async Task MarcarPecaAplicadaAsync(int osPecaId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para marcar peça como aplicada.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var item = await _db.OrdemServicoPecas
                .Include(p => p.OrdemServico)
                .Include(p => p.Peca)
                .FirstOrDefaultAsync(p => p.Id == osPecaId)
                ?? throw new InvalidOperationException("Item de peça não encontrado.");

            if (item.Aplicada)
            {
                return;
            }

            if (item.StatusItem != StatusItem.Aprovado)
            {
                throw new InvalidOperationException($"Apenas peças aprovadas podem ser marcadas como aplicadas (status atual: {item.StatusItem}).");
            }

            var osStatus = item.OrdemServico?.Status;
            if (osStatus != OrdemServicoStatus.EmAndamento &&
                osStatus != OrdemServicoStatus.Pausada &&
                osStatus != OrdemServicoStatus.AguardandoPecas &&
                osStatus != OrdemServicoStatus.AguardandoCliente)
            {
                throw new InvalidOperationException($"Não é permitido aplicar peças com a O.S. em status '{osStatus}'.");
            }

            if (item.Origem == OrigemPeca.Estoque)
            {
                await _estoqueService.BaixarPecaAplicadaAsync(item.Id, usuarioId);
            }

            item.Aplicada = true;
            await _db.SaveChangesAsync();
        });
    }

    public async Task MarcarServicoExecutadoAsync(int osServicoId, int? mecanicoId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para marcar serviço como executado.");
        }

        var item = await _db.OrdemServicoServicos
            .Include(s => s.OrdemServico)
            .FirstOrDefaultAsync(s => s.Id == osServicoId)
            ?? throw new InvalidOperationException("Item de serviço não encontrado.");

        if (item.StatusItem != StatusItem.Aprovado)
        {
            throw new InvalidOperationException($"Apenas serviços aprovados podem ser marcados como executados (status atual: {item.StatusItem}).");
        }

        item.Executado = true;
        if (mecanicoId.HasValue && mecanicoId.Value > 0)
        {
            item.MecanicoId = mecanicoId.Value;
        }
        else if (usuarioId > 0)
        {
            item.MecanicoId = usuarioId;
        }

        await _db.SaveChangesAsync();
    }

    // ==========================================
    // 6. GESTÃO DE ADITIVOS
    // ==========================================

    public async Task<int> AdicionarServicoAditivoAsync(
        int osId,
        int servicoId,
        int quantidade,
        decimal? valorCobrado,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para propor aditivo de serviço.");
        }

        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.ServicosAExecutarList)
            .Include(o => o.Contatos)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status != OrdemServicoStatus.EmAndamento &&
            os.Status != OrdemServicoStatus.Pausada &&
            os.Status != OrdemServicoStatus.AguardandoPecas &&
            os.Status != OrdemServicoStatus.AguardandoCliente)
        {
            throw new InvalidOperationException($"Aditivos só podem ser propostos durante a execução da O.S. (status atual: '{os.Status}').");
        }

        var servico = await _db.Servicos.FindAsync(servicoId)
            ?? throw new InvalidOperationException("Serviço não encontrado.");

        decimal cobrado = valorCobrado ?? servico.ValorPadrao;
        if (cobrado < 0)
        {
            throw new InvalidOperationException("O valor cobrado não pode ser negativo.");
        }

        var item = new OrdemServicoServico
        {
            OrdemServicoId = os.Id,
            ServicoId = servico.Id,
            Quantidade = quantidade,
            PrecoBase = servico.ValorPadrao,
            ValorCobrado = cobrado,
            TempoEstimadoMinutosSnapshot = servico.TempoEstimadoMinutos,
            StatusItem = StatusItem.Pendente,
            EhAditivo = true,
            Executado = false
        };

        _db.OrdemServicoServicos.Add(item);

        if (os.Status != OrdemServicoStatus.AguardandoCliente)
        {
            ValidarTransicao(os.Status, OrdemServicoStatus.AguardandoCliente);
            os.Status = OrdemServicoStatus.AguardandoCliente;
        }

        var operador = await ObterNomeOperadorAsync(usuarioId);

        var contato = new ContatoOS
        {
            OrdemServicoId = os.Id,
            Tipo = TipoContato.Outro,
            Descricao = $"Aditivo proposto: {quantidade}x {servico.Nome} ({cobrado:C}) por {operador}. O.S. movida para Aguardando Cliente.",
            RegistradoPor = operador,
            DataContato = DateTime.UtcNow
        };
        _db.ContatosOS.Add(contato);

        await _db.SaveChangesAsync();
        return item.Id;
    }

    public async Task<int> AdicionarPecaAditivaAsync(
        int osId,
        int pecaId,
        int quantidade,
        decimal? valorCobrado,
        OrigemPeca origem,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para propor aditivo de peça.");
        }

        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.PecasUtilizadas)
            .Include(o => o.Contatos)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status != OrdemServicoStatus.EmAndamento &&
            os.Status != OrdemServicoStatus.Pausada &&
            os.Status != OrdemServicoStatus.AguardandoPecas &&
            os.Status != OrdemServicoStatus.AguardandoCliente)
        {
            throw new InvalidOperationException($"Aditivos só podem ser propostos durante a execução da O.S. (status atual: '{os.Status}').");
        }

        var peca = await _db.Pecas.FindAsync(pecaId)
            ?? throw new InvalidOperationException("Peça não encontrada.");

        decimal cobrado = valorCobrado ?? peca.PrecoVenda;
        if (cobrado < 0)
        {
            throw new InvalidOperationException("O valor cobrado não pode ser negativo.");
        }

        var item = new OrdemServicoPeca
        {
            OrdemServicoId = os.Id,
            PecaId = peca.Id,
            Quantidade = quantidade,
            PrecoBase = peca.PrecoVenda,
            ValorCobrado = cobrado,
            PrecoCustoSnapshot = peca.PrecoCusto,
            Origem = origem,
            StatusItem = StatusItem.Pendente,
            EhAditivo = true,
            Aplicada = false
        };

        _db.OrdemServicoPecas.Add(item);

        if (os.Status != OrdemServicoStatus.AguardandoCliente)
        {
            ValidarTransicao(os.Status, OrdemServicoStatus.AguardandoCliente);
            os.Status = OrdemServicoStatus.AguardandoCliente;
        }

        var operador = await ObterNomeOperadorAsync(usuarioId);

        var contato = new ContatoOS
        {
            OrdemServicoId = os.Id,
            Tipo = TipoContato.Outro,
            Descricao = $"Aditivo proposto: {quantidade}x {peca.Nome} ({cobrado:C}, Origem: {origem}) por {operador}. O.S. movida para Aguardando Cliente.",
            RegistradoPor = operador,
            DataContato = DateTime.UtcNow
        };
        _db.ContatosOS.Add(contato);

        await _db.SaveChangesAsync();
        return item.Id;
    }

    public async Task AprovarAditivoAsync(
        int osId,
        IEnumerable<int>? servicosAprovadosIds,
        IEnumerable<int>? pecasAprovadasIds,
        string? autorizadoPor,
        TipoContato tipoContato,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para aprovar ou recusar aditivos.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.ServicosAExecutarList)
                .Include(o => o.PecasUtilizadas)
                .Include(o => o.Contatos)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            var servicosPendentes = os.ServicosAExecutarList.Where(s => s.EhAditivo && s.StatusItem == StatusItem.Pendente).ToList();
            var pecasPendentes = os.PecasUtilizadas.Where(p => p.EhAditivo && p.StatusItem == StatusItem.Pendente).ToList();

            if (!servicosPendentes.Any() && !pecasPendentes.Any())
            {
                throw new InvalidOperationException("Não há aditivos pendentes de aprovação nesta O.S.");
            }

            var servAprovIds = (servicosAprovadosIds ?? Enumerable.Empty<int>()).ToHashSet();
            var pecasAprovIds = (pecasAprovadasIds ?? Enumerable.Empty<int>()).ToHashSet();

            foreach (var s in servicosPendentes)
            {
                s.StatusItem = servAprovIds.Contains(s.Id) ? StatusItem.Aprovado : StatusItem.Recusado;
            }

            foreach (var p in pecasPendentes)
            {
                if (pecasAprovIds.Contains(p.Id))
                {
                    if (p.Origem == OrigemPeca.Estoque)
                    {
                        var reservaInfo = await _estoqueService.ObterReservaInfoAsync(p.PecaId);
                        if (p.Quantidade > reservaInfo.EstoqueDisponivel)
                        {
                            throw new InvalidOperationException($"Estoque insuficiente para reservar peça aditiva ID {p.PecaId} (disponível: {reservaInfo.EstoqueDisponivel}, solicitado: {p.Quantidade}).");
                        }
                    }
                    p.StatusItem = StatusItem.Aprovado;
                }
                else
                {
                    p.StatusItem = StatusItem.Recusado;
                }
            }

            if (os.Status == OrdemServicoStatus.AguardandoCliente)
            {
                ValidarTransicao(os.Status, OrdemServicoStatus.EmAndamento);
                os.Status = OrdemServicoStatus.EmAndamento;
            }

            var operador = await ObterNomeOperadorAsync(usuarioId);
            var autorizador = string.IsNullOrWhiteSpace(autorizadoPor) ? "Cliente" : autorizadoPor.Trim();

            var contato = new ContatoOS
            {
                OrdemServicoId = os.Id,
                Tipo = tipoContato,
                Descricao = $"Aditivo respondido por {autorizador} via {tipoContato}. Retornada para Em Andamento por {operador}.",
                RegistradoPor = operador,
                DataContato = DateTime.UtcNow
            };
            _db.ContatosOS.Add(contato);

            await _db.SaveChangesAsync();
        });
    }

    // ==========================================
    // 7. CONCLUSÃO DA ORDEM DE SERVIÇO
    // ==========================================

    public Task ConcluirOSAsync(int osId, PerfilUsuario? perfilUsuario, int usuarioId = 0) =>
        ConcluirOSAsync(osId, null, null, usuarioId, perfilUsuario);

    public async Task ConcluirOSAsync(
        int osId,
        string? checklistConclusaoJson = null,
        string? justificativaItensNaoExecutados = null,
        int usuarioId = 0,
        PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Apenas Administradores, Atendimento e Mecânicos possuem permissão para concluir ordens de serviço.");
        }

        var os = await _db.OrdensServico
            .Include(o => o.ApontamentosTempo)
            .Include(o => o.ServicosAExecutarList)
            .Include(o => o.PecasUtilizadas)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.Concluida);

        // Bloquear se houver aditivos pendentes
        var temAditivoPendente = os.ServicosAExecutarList.Any(s => s.EhAditivo && s.StatusItem == StatusItem.Pendente)
            || os.PecasUtilizadas.Any(p => p.EhAditivo && p.StatusItem == StatusItem.Pendente);

        if (temAditivoPendente)
        {
            throw new InvalidOperationException("Não é possível concluir a O.S.: existem itens aditivos pendentes de aprovação pelo cliente.");
        }

        // Exigir que todos os itens aprovados estejam executados/aplicados (ou justificativa formal)
        var servicosNaoExecutados = os.ServicosAExecutarList.Where(s => s.StatusItem == StatusItem.Aprovado && !s.Executado).ToList();
        var pecasNaoAplicadas = os.PecasUtilizadas.Where(p => p.StatusItem == StatusItem.Aprovado && !p.Aplicada).ToList();

        if ((servicosNaoExecutados.Any() || pecasNaoAplicadas.Any()) && string.IsNullOrWhiteSpace(justificativaItensNaoExecutados))
        {
            throw new InvalidOperationException(
                "Existem itens aprovados não executados ou peças não aplicadas. Marque-os como executados/aplicados ou informe uma justificativa formal para a conclusão.");
        }

        // Fechar apontamentos abertos
        FecharApontamentosAbertos(os);

        // Persistir checklist de conclusão (fluidos, limpeza, ferramentas etc.)
        if (!string.IsNullOrWhiteSpace(checklistConclusaoJson))
        {
            os.ChecklistConclusaoJson = checklistConclusaoJson;
        }

        os.Status = OrdemServicoStatus.Concluida;
        os.DataConclusao = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // ==========================================
    // 8. ENTREGA DO VEÍCULO & PAGAMENTO
    // ==========================================

    public async Task EntregarOSAsync(
        int osId,
        int kmSaida,
        IEnumerable<Pagamento>? pagamentos,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null,
        string? observacaoSaldoEmAberto = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para registrar a entrega do veículo.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.Veiculo)
                .Include(o => o.Pagamentos)
                .Include(o => o.ServicosAExecutarList)
                .Include(o => o.PecasUtilizadas)
                .Include(o => o.Contatos)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status != OrdemServicoStatus.Concluida)
            {
                throw new InvalidOperationException($"Apenas O.S. no status 'Concluida' podem ser entregues (status atual: '{os.Status}').");
            }

            ValidarTransicao(os.Status, OrdemServicoStatus.Entregue);

            if (kmSaida < os.KmEntrada)
            {
                throw new InvalidOperationException($"O KM de saída ({kmSaida}) não pode ser menor que o KM de entrada ({os.KmEntrada}).");
            }

            var listaPagamentos = pagamentos?.ToList() ?? new List<Pagamento>();
            var totalJaPago = os.Pagamentos.Sum(p => p.Valor);
            var totalNovos = listaPagamentos.Sum(p => p.Valor);
            var totalRecebido = totalJaPago + totalNovos;

            if (totalRecebido < os.ValorTotal)
            {
                if (perfil != PerfilUsuario.Administrador)
                {
                    throw new InvalidOperationException(
                        $"Valor pago insuficiente ({totalRecebido:C} de {os.ValorTotal:C}). Apenas Administradores podem autorizar a entrega com saldo em aberto.");
                }

                var operador = await ObterNomeOperadorAsync(usuarioId);
                var obs = string.IsNullOrWhiteSpace(observacaoSaldoEmAberto)
                    ? $"Entrega autorizada com saldo em aberto de {(os.ValorTotal - totalRecebido):C} pelo Administrador {operador}."
                    : observacaoSaldoEmAberto.Trim();

                var contatoSaldo = new ContatoOS
                {
                    OrdemServicoId = os.Id,
                    Tipo = TipoContato.Outro,
                    Descricao = obs,
                    RegistradoPor = operador,
                    DataContato = DateTime.UtcNow
                };
                _db.ContatosOS.Add(contatoSaldo);
            }

            foreach (var pag in listaPagamentos)
            {
                if (pag.Valor <= 0)
                {
                    throw new InvalidOperationException("O valor do pagamento deve ser maior que zero.");
                }

                pag.OrdemServicoId = os.Id;
                pag.DataPagamento = DateTime.UtcNow;
                pag.RegistradoPorUsuarioId = usuarioId;

                _db.Pagamentos.Add(pag);
            }

            var config = await _db.Configuracoes.FirstOrDefaultAsync();
            os.GarantiaDias = config?.GarantiaPadraoDias ?? 90;
            os.DataEntrega = DateTime.UtcNow;
            os.KmSaida = kmSaida;

            if (os.Veiculo != null)
            {
                os.Veiculo.Quilometragem = kmSaida;
            }

            os.Status = OrdemServicoStatus.Entregue;

            await _db.SaveChangesAsync();
        });
    }

    // ==========================================
    // 9. EDIÇÃO / ATUALIZAÇÃO DA O.S.
    // ==========================================

    public async Task AtualizarOSAsync(
        int osId,
        string? queixaCliente,
        string? diagnostico,
        DateTime dataPrevisaoEntrega,
        decimal valorDesconto,
        int usuarioId,
        PerfilUsuario? perfilUsuario = null,
        string? nomeUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para editar ordens de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .Include(o => o.ServicosAExecutarList)
                .Include(o => o.Contatos)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status == OrdemServicoStatus.Concluida
                || os.Status == OrdemServicoStatus.Entregue
                || os.Status == OrdemServicoStatus.Cancelada
                || os.Status == OrdemServicoStatus.Recusada)
            {
                throw new InvalidOperationException(
                    $"Não é permitido editar uma O.S. com status '{os.Status}' (somente leitura).");
            }

            if (dataPrevisaoEntrega.Date < os.DataEmissao.Date)
            {
                throw new InvalidOperationException(
                    "A data de previsão de entrega não pode ser anterior à data de emissão.");
            }

            if (os.Status == OrdemServicoStatus.Orcamento)
            {
                if (string.IsNullOrWhiteSpace(queixaCliente))
                {
                    throw new InvalidOperationException("A queixa do cliente é obrigatória.");
                }

                if (valorDesconto < 0)
                {
                    throw new InvalidOperationException("O valor de desconto não pode ser negativo.");
                }

                if (valorDesconto != os.ValorDesconto && perfil.Value != PerfilUsuario.Administrador)
                {
                    throw new UnauthorizedAccessException(
                        "Apenas Administradores podem definir ou alterar o desconto da Ordem de Serviço.");
                }

                var subtotalBruto = os.ValorMaoDeObraEfetivo + os.ValorPecasEfetivo;

                if (valorDesconto > subtotalBruto)
                {
                    throw new InvalidOperationException(
                        $"O desconto ({valorDesconto:C}) não pode ser maior que o valor bruto da Ordem de Serviço ({subtotalBruto:C}).");
                }

                os.QueixaCliente = queixaCliente.Trim();
                os.Diagnostico = string.IsNullOrWhiteSpace(diagnostico) ? null : diagnostico.Trim();
                os.DataPrevisaoEntrega = dataPrevisaoEntrega.Date;
                os.ValorDesconto = valorDesconto;

                await _db.SaveChangesAsync();
                return;
            }

            // Pós-aprovação: apenas Diagnóstico e Previsão de Entrega
            var queixaNorm = (queixaCliente ?? string.Empty).Trim();
            var queixaAtual = (os.QueixaCliente ?? string.Empty).Trim();

            if (queixaNorm != queixaAtual || valorDesconto != os.ValorDesconto)
            {
                throw new InvalidOperationException(
                    "Após a aprovação do orçamento, apenas a Previsão de Entrega e o Diagnóstico podem ser alterados.");
            }

            var diagNorm = string.IsNullOrWhiteSpace(diagnostico) ? null : diagnostico.Trim();
            var houveAlteracao = false;

            if (diagNorm != os.Diagnostico)
            {
                os.Diagnostico = diagNorm;
                houveAlteracao = true;
            }

            if (dataPrevisaoEntrega.Date != os.DataPrevisaoEntrega.Date)
            {
                var dataAntiga = os.DataPrevisaoEntrega.ToString("dd/MM/yyyy");
                var dataNova = dataPrevisaoEntrega.Date.ToString("dd/MM/yyyy");
                os.DataPrevisaoEntrega = dataPrevisaoEntrega.Date;

                var nomeOperador = nomeUsuario?.Trim() ?? await ObterNomeOperadorAsync(usuarioId);

                var contato = new ContatoOS
                {
                    OrdemServicoId = os.Id,
                    Tipo = TipoContato.Outro,
                    Descricao = $"Previsão alterada de {dataAntiga} para {dataNova} por {nomeOperador}",
                    RegistradoPor = nomeOperador,
                    DataContato = DateTime.UtcNow
                };

                _db.ContatosOS.Add(contato);
                houveAlteracao = true;
            }

            if (houveAlteracao)
            {
                await _db.SaveChangesAsync();
            }
        });
    }

    // ==========================================
    // CANCELAMENTO DA O.S.
    // ==========================================

    public async Task CancelarOSAsync(int osId, string? motivo, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para cancelar ordens de serviço.");
        }

        var motivoLimpo = motivo?.Trim() ?? string.Empty;
        if (motivoLimpo.Length < 5)
        {
            throw new InvalidOperationException("O motivo do cancelamento é obrigatório e deve ter pelo menos 5 caracteres.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .Include(o => o.ApontamentosTempo)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            var statusAnterior = os.Status;
            ValidarTransicao(statusAnterior, OrdemServicoStatus.Cancelada);

            // Estorna apenas peças que foram fisicamente aplicadas; reservas não aplicadas são liberadas automaticamente
            if (os.PecasUtilizadas.Any(p => p.Aplicada))
            {
                await _estoqueService.EstornarPecasCancelamentoOSAsync(os.Id, usuarioId);
            }

            FecharApontamentosAbertos(os);
            os.Status = OrdemServicoStatus.Cancelada;
            os.MotivoCancelamento = motivoLimpo;
            os.DataCancelamento = DateTime.UtcNow;
            os.CanceladaPorUsuarioId = usuarioId;

            await _db.SaveChangesAsync();
        });
    }

    public static bool EstaComEstoqueBaixado(OrdemServicoStatus status)
    {
        return status == OrdemServicoStatus.Aprovada
            || status == OrdemServicoStatus.EmAndamento
            || status == OrdemServicoStatus.Pausada
            || status == OrdemServicoStatus.AguardandoPecas
            || status == OrdemServicoStatus.AguardandoCliente;
    }

    public async Task AdicionarPecaAsync(int osId, int pecaId, int quantidade, decimal? valorCobrado, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
        }

        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil))
        {
            throw new UnauthorizedAccessException("Usuário sem permissão para adicionar peças à ordem de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status == OrdemServicoStatus.Concluida ||
                os.Status == OrdemServicoStatus.Entregue ||
                os.Status == OrdemServicoStatus.Cancelada ||
                os.Status == OrdemServicoStatus.Recusada)
            {
                throw new InvalidOperationException($"Não é permitido adicionar peças a uma O.S. com status '{os.Status}'.");
            }

            var peca = await _db.Pecas.FindAsync(pecaId)
                ?? throw new InvalidOperationException("Peça não encontrada.");

            var existente = os.PecasUtilizadas.FirstOrDefault(p => p.PecaId == pecaId);
            decimal cobrado = valorCobrado ?? existente?.ValorCobrado ?? peca.PrecoVenda;
            if (cobrado < 0)
            {
                throw new InvalidOperationException("O valor cobrado não pode ser negativo.");
            }

            if (cobrado < peca.PrecoVenda && perfil != PerfilUsuario.Administrador)
            {
                throw new UnauthorizedAccessException("Apenas Administradores podem aplicar desconto abaixo do preço base sugerido.");
            }

            if (existente != null)
            {
                existente.Quantidade += quantidade;
                if (valorCobrado.HasValue)
                {
                    existente.ValorCobrado = valorCobrado.Value;
                }
            }
            else
            {
                var osPeca = new OrdemServicoPeca
                {
                    OrdemServicoId = os.Id,
                    PecaId = peca.Id,
                    Quantidade = quantidade,
                    PrecoBase = peca.PrecoVenda,
                    ValorCobrado = cobrado,
                    PrecoCustoSnapshot = peca.PrecoCusto,
                    Origem = OrigemPeca.Estoque,
                    StatusItem = os.Status == OrdemServicoStatus.Orcamento ? StatusItem.Pendente : StatusItem.Aprovado,
                    Aplicada = false
                };
                _db.OrdemServicoPecas.Add(osPeca);
            }

            await _db.SaveChangesAsync();
        });
    }

    public async Task RemoverPecaAsync(int osId, int osPecaId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (!EhPerfilValido(perfil) || (perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento))
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para remover peças da ordem de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status == OrdemServicoStatus.Concluida ||
                os.Status == OrdemServicoStatus.Entregue ||
                os.Status == OrdemServicoStatus.Cancelada ||
                os.Status == OrdemServicoStatus.Recusada)
            {
                throw new InvalidOperationException($"Não é permitido remover peças de uma O.S. com status '{os.Status}'.");
            }

            var item = os.PecasUtilizadas.FirstOrDefault(p => p.Id == osPecaId)
                ?? throw new InvalidOperationException("Peça vinculada não encontrada nesta O.S.");

            if (item.Aplicada && item.Origem == OrigemPeca.Estoque)
            {
                await _estoqueService.EstornarPecaIndividualDaOSAsync(os.Id, item.PecaId, item.Quantidade, usuarioId);
            }

            os.PecasUtilizadas.Remove(item);
            _db.OrdemServicoPecas.Remove(item);

            await _db.SaveChangesAsync();
        });
    }

    public static TipoContato? ConverterMeioAutorizacao(string? meioAutorizacao)
    {
        if (string.IsNullOrWhiteSpace(meioAutorizacao))
        {
            return null;
        }

        var texto = meioAutorizacao.Trim();
        if (texto.Contains("presencial", StringComparison.OrdinalIgnoreCase))
            return TipoContato.Presencial;
        if (texto.Contains("whats", StringComparison.OrdinalIgnoreCase))
            return TipoContato.WhatsApp;
        if (texto.Contains("telefone", StringComparison.OrdinalIgnoreCase)
            || texto.Contains("ligac", StringComparison.OrdinalIgnoreCase)
            || texto.Contains("ligaç", StringComparison.OrdinalIgnoreCase)
            || texto.Contains("fone", StringComparison.OrdinalIgnoreCase))
            return TipoContato.Ligacao;
        if (texto.Contains("e-mail", StringComparison.OrdinalIgnoreCase)
            || texto.Contains("email", StringComparison.OrdinalIgnoreCase))
            return TipoContato.Email;

        return TipoContato.Outro;
    }

    private async Task<string> ObterNomeOperadorAsync(int usuarioId)
    {
        if (usuarioId <= 0) return "Usuário";
        return await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => u.NomeCompleto)
            .FirstOrDefaultAsync() ?? "Usuário";
    }

    private async Task<PerfilUsuario?> ObterPerfilAsync(int usuarioId, PerfilUsuario? perfilInformado)
    {
        if (perfilInformado.HasValue)
        {
            return perfilInformado.Value;
        }

        return await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => (PerfilUsuario?)u.Perfil)
            .FirstOrDefaultAsync();
    }

    private async Task ExecutarEmTransacaoAsync(Func<Task> acao)
    {
        var possuiTransacaoExterna = _db.Database.CurrentTransaction != null;
        var eRelacional = _db.Database.IsRelational();
        await using var transacao = (!possuiTransacaoExterna && eRelacional)
            ? await _db.Database.BeginTransactionAsync()
            : null;

        try
        {
            await acao();

            if (transacao != null)
            {
                await transacao.CommitAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (transacao != null)
            {
                await transacao.RollbackAsync();
                _db.ChangeTracker.Clear();
            }

            throw new InvalidOperationException(
                "Conflito de concorrência: o estoque ou a O.S. foi alterada simultaneamente por outro usuário. Atualize a página e tente novamente.",
                ex);
        }
        catch
        {
            if (transacao != null)
            {
                await transacao.RollbackAsync();
                _db.ChangeTracker.Clear();
            }
            throw;
        }
    }

    private static void FecharApontamentosAbertos(OrdemServico os)
    {
        if (os.ApontamentosTempo == null)
        {
            return;
        }

        var agora = DateTime.UtcNow;
        foreach (var apontamento in os.ApontamentosTempo.Where(a => !a.Fim.HasValue))
        {
            apontamento.Fim = agora;
        }
    }
}
