using System.Globalization;
using System.Text;
using mechsystem.Data;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services;

public record ResumoConcluidasDto(
    int TotalConcluidas,
    decimal ReceitaMaoDeObra,
    decimal ReceitaPecas,
    decimal TotalDescontos,
    decimal FaturamentoTotal,
    decimal TicketMedio,
    decimal LucroRealPecas);

public record ResumoFunilDto(
    int TotalOS,
    int TotalOrcamentos,
    int TotalAguardandoPecas,
    int TotalEmAndamento,
    int TotalPausadas,
    int TotalConcluidasEmitidas,
    int TotalCanceladas,
    int TotalAprovadas,
    double TaxaConversao);

public record ResumoEstoqueDto(
    int TotalPecasCadastradas,
    int PecasAtivas,
    int PecasInativas,
    int PecasAbaixoDoMinimo,
    decimal CapitalImobilizado,
    decimal MargemLucroMedia,
    int TotalMovimentacoesPeriodo,
    int EntradasPeriodo,
    int SaidasPeriodo,
    int AjustesPeriodo,
    List<Peca> PecasEmRuptura);

public record OrdemServicoPeriodoItemDto(
    int Id,
    DateTime DataEmissao,
    DateTime? DataConclusao,
    string Placa,
    string VeiculoDescricao,
    string ClienteNome,
    OrdemServicoStatus Status,
    decimal ValorMaoDeObraEfetivo,
    decimal ValorPecasEfetivo,
    decimal ValorDesconto,
    decimal ValorTotal);

public record RankingServicoItemDto(
    string NomeServico,
    int QuantidadeExecucoes,
    decimal ReceitaTotal);

public record MovimentacaoPeriodoItemDto(
    int Id,
    DateTime DataHora,
    string Sku,
    string PecaNome,
    TipoMovimentacao Tipo,
    int Quantidade,
    string? Referencia,
    string UsuarioNome);

public class RelatorioService
{
    private readonly AppDbContext _db;
    private readonly EstoqueService _estoqueService;

    public RelatorioService(AppDbContext db, EstoqueService estoqueService)
    {
        _db = db;
        _estoqueService = estoqueService;
    }

    /// <summary>
    /// Calcula métricas de O.S. concluídas por DataConclusao no banco de dados,
    /// preservando exatamente a regra de fallback de ValorMaoDeObraEfetivo e ValorPecasEfetivo.
    /// </summary>
    public async Task<ResumoConcluidasDto> ObterMetricasConcluidasAsync(DateTime? inicioUtc, DateTime? fimUtc)
    {
        var query = _db.OrdensServico
            .AsNoTracking()
            .Where(o => o.Status == OrdemServicoStatus.Concluida);

        if (inicioUtc.HasValue)
        {
            query = query.Where(o => o.DataConclusao >= inicioUtc.Value);
        }
        if (fimUtc.HasValue)
        {
            query = query.Where(o => o.DataConclusao < fimUtc.Value);
        }

        var itens = await query
            .Select(o => new
            {
                MaoDeObraEfetiva = o.ServicosAExecutarList.Any()
                    ? o.ServicosAExecutarList.Sum(s => (decimal?)(s.Quantidade * s.ValorCobrado)) ?? 0m
                    : o.ValorMaoDeObra,
                PecasEfetiva = o.PecasUtilizadas.Any()
                    ? o.PecasUtilizadas.Sum(p => (decimal?)(p.Quantidade * p.ValorCobrado)) ?? 0m
                    : o.ValorPecas,
                Desconto = o.ValorDesconto,
                LucroPecas = o.PecasUtilizadas.Sum(p => (decimal?)(p.Quantidade * (p.ValorCobrado - p.PrecoCustoSnapshot))) ?? 0m
            })
            .ToListAsync();

        var totalConcluidas = itens.Count;
        var receitaMaoDeObra = itens.Sum(x => x.MaoDeObraEfetiva);
        var receitaPecas = itens.Sum(x => x.PecasEfetiva);
        var totalDescontos = itens.Sum(x => x.Desconto);
        var faturamentoTotal = (receitaMaoDeObra + receitaPecas) - totalDescontos;
        var ticketMedio = totalConcluidas > 0 ? faturamentoTotal / totalConcluidas : 0m;
        var lucroRealPecas = itens.Sum(x => x.LucroPecas);

        return new ResumoConcluidasDto(
            totalConcluidas,
            receitaMaoDeObra,
            receitaPecas,
            totalDescontos,
            faturamentoTotal,
            ticketMedio,
            lucroRealPecas);
    }

    /// <summary>
    /// Cálculo equivalente em memória sobre entidades OrdemServico materializadas (para validação/testes de paridade).
    /// </summary>
    public static ResumoConcluidasDto CalcularMetricasConcluidasEmMemoria(IEnumerable<OrdemServico> ordensConcluidas)
    {
        var lista = ordensConcluidas.ToList();
        var totalConcluidas = lista.Count;
        var receitaMaoDeObra = lista.Sum(o => o.ValorMaoDeObraEfetivo);
        var receitaPecas = lista.Sum(o => o.ValorPecasEfetivo);
        var totalDescontos = lista.Sum(o => o.ValorDesconto);
        var faturamentoTotal = lista.Sum(o => o.ValorTotal);
        var ticketMedio = totalConcluidas > 0 ? faturamentoTotal / totalConcluidas : 0m;
        var lucroRealPecas = lista
            .SelectMany(o => o.PecasUtilizadas ?? Enumerable.Empty<OrdemServicoPeca>())
            .Sum(p => p.Quantidade * (p.ValorCobrado - p.PrecoCustoSnapshot));

        return new ResumoConcluidasDto(
            totalConcluidas,
            receitaMaoDeObra,
            receitaPecas,
            totalDescontos,
            faturamentoTotal,
            ticketMedio,
            lucroRealPecas);
    }

    /// <summary>
    /// Agrega no banco o funil de O.S. emitidas no período selecionado.
    /// </summary>
    public async Task<ResumoFunilDto> ObterFunilPeriodoAsync(DateTime? inicioUtc, DateTime? fimUtc)
    {
        var query = _db.OrdensServico.AsNoTracking().AsQueryable();
        if (inicioUtc.HasValue)
        {
            query = query.Where(o => o.DataEmissao >= inicioUtc.Value);
        }
        if (fimUtc.HasValue)
        {
            query = query.Where(o => o.DataEmissao < fimUtc.Value);
        }

        var contagens = await query
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        int Contar(OrdemServicoStatus status) =>
            contagens.FirstOrDefault(c => c.Status == status)?.Quantidade ?? 0;

        var orcamentos = Contar(OrdemServicoStatus.Orcamento);
        var aguardandoPecas = Contar(OrdemServicoStatus.AguardandoPecas);
        var emAndamento = Contar(OrdemServicoStatus.EmAndamento);
        var pausadas = Contar(OrdemServicoStatus.Pausada);
        var concluidasEmitidas = Contar(OrdemServicoStatus.Concluida);
        var canceladas = Contar(OrdemServicoStatus.Cancelada);
        var totalOS = contagens.Sum(c => c.Quantidade);

        var totalAprovadas = aguardandoPecas + emAndamento + pausadas + concluidasEmitidas;
        var totalDecididas = totalAprovadas + canceladas;
        var taxaConversao = totalDecididas > 0 ? (totalAprovadas * 100.0) / totalDecididas : 0.0;

        return new ResumoFunilDto(
            totalOS,
            orcamentos,
            aguardandoPecas,
            emAndamento,
            pausadas,
            concluidasEmitidas,
            canceladas,
            totalAprovadas,
            taxaConversao);
    }

    /// <summary>
    /// Obtém posição atual do estoque (tempo real) separada das movimentações ocorridas no período.
    /// </summary>
    public async Task<ResumoEstoqueDto> ObterPosicaoEPeriodoEstoqueAsync(DateTime? inicioUtc, DateTime? fimUtc)
    {
        var pecasEmRuptura = await _estoqueService.GetAlertasRupturaAsync();
        var capitalImobilizado = await _estoqueService.GetCapitalImobilizadoAsync();
        var pecasAtivas = await _db.Pecas.Ativos().CountAsync();
        var pecasInativas = await _db.Pecas.Inativos().CountAsync();
        var totalPecas = pecasAtivas + pecasInativas;

        var precosAtivas = await _db.Pecas
            .AsNoTracking()
            .Ativos()
            .Where(p => p.PrecoVenda > 0)
            .Select(p => new { p.PrecoVenda, p.PrecoCusto })
            .ToListAsync();

        var margemMedia = precosAtivas.Any()
            ? (decimal)precosAtivas.Average(p => ((double)(p.PrecoVenda - p.PrecoCusto) / (double)p.PrecoVenda) * 100.0)
            : 0m;

        var queryMov = _db.MovimentacoesEstoque.AsNoTracking().AsQueryable();
        if (inicioUtc.HasValue)
        {
            queryMov = queryMov.Where(m => m.DataHora >= inicioUtc.Value);
        }
        if (fimUtc.HasValue)
        {
            queryMov = queryMov.Where(m => m.DataHora < fimUtc.Value);
        }

        var movContagens = await queryMov
            .GroupBy(m => m.Tipo)
            .Select(g => new { Tipo = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        int ContarMov(TipoMovimentacao tipo) =>
            movContagens.FirstOrDefault(m => m.Tipo == tipo)?.Quantidade ?? 0;

        var entradas = ContarMov(TipoMovimentacao.Entrada);
        var saidas = ContarMov(TipoMovimentacao.Saida);
        var ajustes = ContarMov(TipoMovimentacao.Ajuste);
        var totalMov = movContagens.Sum(m => m.Quantidade);

        return new ResumoEstoqueDto(
            totalPecas,
            pecasAtivas,
            pecasInativas,
            pecasEmRuptura.Count,
            capitalImobilizado,
            margemMedia,
            totalMov,
            entradas,
            saidas,
            ajustes,
            pecasEmRuptura);
    }

    /// <summary>
    /// Gera a lista polimórfica de relatórios (RelatorioOS, RelatorioEstoque e opcionalmente RelatorioFinanceiro)
    /// compartilhada entre Home e Relatórios.
    /// </summary>
    public async Task<(List<Relatorio> Relatorios, ResumoConcluidasDto Concluidas, ResumoFunilDto Funil, ResumoEstoqueDto Estoque)> GerarPacoteRelatoriosAsync(
        DateTime? inicioUtc,
        DateTime? fimUtc,
        string geradoPor = "Sistema",
        bool incluirFinanceiro = true)
    {
        var concluidas = await ObterMetricasConcluidasAsync(inicioUtc, fimUtc);
        var funil = await ObterFunilPeriodoAsync(inicioUtc, fimUtc);
        var estoque = await ObterPosicaoEPeriodoEstoqueAsync(inicioUtc, fimUtc);
        var agoraUtc = DateTime.UtcNow;
        var autor = string.IsNullOrWhiteSpace(geradoPor) ? "Sistema" : geradoPor.Trim();

        var lista = new List<Relatorio>
        {
            new RelatorioOS
            {
                Titulo = "Ordens de Serviço",
                DataGeracao = agoraUtc,
                GeradoPor = autor,
                TotalOS = funil.TotalOS,
                TotalConcluidas = concluidas.TotalConcluidas,
                TotalCanceladas = funil.TotalCanceladas,
                TotalEmAndamento = funil.TotalEmAndamento,
                TotalOrcamentos = funil.TotalOrcamentos,
                FaturamentoTotal = concluidas.FaturamentoTotal,
                TicketMedio = concluidas.TicketMedio,
                TaxaConversao = funil.TaxaConversao
            },
            new RelatorioEstoque
            {
                Titulo = "Controle de Estoque",
                DataGeracao = agoraUtc,
                GeradoPor = autor,
                TotalPecasCadastradas = estoque.TotalPecasCadastradas,
                PecasAtivas = estoque.PecasAtivas,
                PecasInativas = estoque.PecasInativas,
                PecasAbaixoDoMinimo = estoque.PecasAbaixoDoMinimo,
                CapitalImobilizado = estoque.CapitalImobilizado,
                MargemLucroMedia = estoque.MargemLucroMedia,
                TotalMovimentacoes = estoque.TotalMovimentacoesPeriodo,
                MovimentacoesEntradaPeriodo = estoque.EntradasPeriodo,
                MovimentacoesSaidaPeriodo = estoque.SaidasPeriodo,
                MovimentacoesAjustePeriodo = estoque.AjustesPeriodo
            }
        };

        if (incluirFinanceiro)
        {
            lista.Add(new RelatorioFinanceiro
            {
                Titulo = "Panorama Financeiro",
                DataGeracao = agoraUtc,
                GeradoPor = autor,
                ReceitaMaoDeObra = concluidas.ReceitaMaoDeObra,
                ReceitaPecas = concluidas.ReceitaPecas,
                ReceitaTotal = concluidas.FaturamentoTotal,
                LucroRealPecas = concluidas.LucroRealPecas,
                CapitalImobilizado = estoque.CapitalImobilizado,
                TotalOSPagas = concluidas.TotalConcluidas
            });
        }

        return (lista, concluidas, funil, estoque);
    }

    /// <summary>
    /// Lista detalhada das O.S. do período (emitidas ou concluídas no intervalo) para a tabela de detalhes e exportação CSV.
    /// </summary>
    public async Task<List<OrdemServicoPeriodoItemDto>> ObterOrdensDoPeriodoDetalheAsync(DateTime? inicioUtc, DateTime? fimUtc)
    {
        var query = _db.OrdensServico
            .AsNoTracking()
            .Include(o => o.Veiculo)
                .ThenInclude(v => v!.Cliente)
            .Include(o => o.PecasUtilizadas)
            .Include(o => o.ServicosAExecutarList)
            .AsQueryable();

        if (inicioUtc.HasValue && fimUtc.HasValue)
        {
            var ini = inicioUtc.Value;
            var fim = fimUtc.Value;
            query = query.Where(o =>
                (o.DataEmissao >= ini && o.DataEmissao < fim) ||
                (o.Status == OrdemServicoStatus.Concluida && o.DataConclusao >= ini && o.DataConclusao < fim));
        }

        var ordens = await query
            .OrderByDescending(o => o.DataEmissao)
            .ThenByDescending(o => o.Id)
            .ToListAsync();

        return ordens.Select(o => new OrdemServicoPeriodoItemDto(
            o.Id,
            o.DataEmissao,
            o.DataConclusao,
            o.Veiculo?.Placa ?? "—",
            $"{o.Veiculo?.Marca} {o.Veiculo?.Modelo}".Trim(),
            o.Veiculo?.Cliente?.Nome ?? "—",
            o.Status,
            o.ValorMaoDeObraEfetivo,
            o.ValorPecasEfetivo,
            o.ValorDesconto,
            o.ValorTotal)).ToList();
    }

    /// <summary>
    /// Ranking de serviços realizados no período por quantidade e receita (combina OrdemServicoServicos e fallback textual).
    /// </summary>
    public async Task<List<RankingServicoItemDto>> ObterRankingServicosPeriodoAsync(DateTime? inicioUtc, DateTime? fimUtc)
    {
        var query = _db.OrdensServico
            .AsNoTracking()
            .Include(o => o.ServicosAExecutarList)
                .ThenInclude(s => s.Servico)
            .Where(o => o.Status != OrdemServicoStatus.Cancelada);

        if (inicioUtc.HasValue && fimUtc.HasValue)
        {
            var ini = inicioUtc.Value;
            var fim = fimUtc.Value;
            query = query.Where(o =>
                (o.Status == OrdemServicoStatus.Concluida && o.DataConclusao >= ini && o.DataConclusao < fim) ||
                (o.Status != OrdemServicoStatus.Concluida && o.DataEmissao >= ini && o.DataEmissao < fim));
        }

        var ordens = await query.ToListAsync();
        var acumulador = new Dictionary<string, (string NomeExibicao, int Quantidade, decimal Receita)>(StringComparer.OrdinalIgnoreCase);

        foreach (var os in ordens)
        {
            if (os.ServicosAExecutarList != null && os.ServicosAExecutarList.Any())
            {
                foreach (var item in os.ServicosAExecutarList)
                {
                    var nome = item.Servico?.Nome?.Trim();
                    if (string.IsNullOrWhiteSpace(nome))
                        nome = "Serviço Geral";

                    var chave = nome.ToUpperInvariant();
                    if (acumulador.TryGetValue(chave, out var atual))
                    {
                        acumulador[chave] = (atual.NomeExibicao, atual.Quantidade + item.Quantidade, atual.Receita + item.Subtotal);
                    }
                    else
                    {
                        acumulador[chave] = (nome, item.Quantidade, item.Subtotal);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(os.ServicoAExecutar))
            {
                var nome = os.ServicoAExecutar.Trim();
                var chave = nome.ToUpperInvariant();
                if (acumulador.TryGetValue(chave, out var atual))
                {
                    acumulador[chave] = (atual.NomeExibicao, atual.Quantidade + 1, atual.Receita + os.ValorMaoDeObraEfetivo);
                }
                else
                {
                    acumulador[chave] = (nome, 1, os.ValorMaoDeObraEfetivo);
                }
            }
        }

        return acumulador.Values
            .OrderByDescending(x => x.Quantidade)
            .ThenByDescending(x => x.Receita)
            .Select(x => new RankingServicoItemDto(x.NomeExibicao, x.Quantidade, x.Receita))
            .ToList();
    }

    /// <summary>
    /// Lista detalhada das movimentações de estoque do período com identificação do usuário operador.
    /// </summary>
    public async Task<List<MovimentacaoPeriodoItemDto>> ObterMovimentacoesPeriodoDetalheAsync(DateTime? inicioUtc, DateTime? fimUtc)
    {
        var query = _db.MovimentacoesEstoque
            .AsNoTracking()
            .Include(m => m.Peca)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (inicioUtc.HasValue)
        {
            query = query.Where(m => m.DataHora >= inicioUtc.Value);
        }
        if (fimUtc.HasValue)
        {
            query = query.Where(m => m.DataHora < fimUtc.Value);
        }

        var lista = await query
            .OrderByDescending(m => m.DataHora)
            .ThenByDescending(m => m.Id)
            .ToListAsync();

        return lista.Select(m => new MovimentacaoPeriodoItemDto(
            m.Id,
            m.DataHora,
            m.Peca?.Sku ?? "—",
            m.Peca?.Nome ?? "—",
            m.Tipo,
            m.Quantidade,
            m.Referencia,
            m.Usuario?.NomeCompleto ?? "Sistema")).ToList();
    }

    /// <summary>
    /// Formata uma tabela CSV com separador ';' e valores em formato pt-BR (para download UTF-8 com BOM).
    /// </summary>
    public static string GerarCsvPtBr(IEnumerable<string> cabecalhos, IEnumerable<IEnumerable<string?>> linhas)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", cabecalhos.Select(EscaparCampoCsv)));

        foreach (var linha in linhas)
        {
            sb.AppendLine(string.Join(";", linha.Select(EscaparCampoCsv)));
        }

        return sb.ToString();
    }

    private static string EscaparCampoCsv(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;

        var limpo = valor.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim();
        if (limpo.Contains(';') || limpo.Contains('"'))
        {
            return $"\"{limpo.Replace("\"", "\"\"")}\"";
        }
        return limpo;
    }
}
