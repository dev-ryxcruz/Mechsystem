using mechsystem.Data;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services.Queries;

public record MarcaRankingDto(string Nome, int Quantidade);

public record DashboardIndicadoresDto(
    int TotalVeiculos,
    decimal IdadeMediaFrota,
    int TotalEmAndamento,
    decimal DinheiroNaMesa,
    int TotalAtrasadas,
    List<MarcaRankingDto> TopMarcas);

public class OrdemServicoQueryService
{
    private readonly AppDbContext _db;

    public OrdemServicoQueryService(AppDbContext db)
    {
        _db = db;
    }

    public Task<OrdemServico?> ObterDetalhesAsync(int id)
    {
        return _db.OrdensServico
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.Vistoria)
            .Include(o => o.Veiculo)
                .ThenInclude(v => v!.Cliente)
            .Include(o => o.PecasUtilizadas)
                .ThenInclude(p => p.Peca)
            .Include(o => o.ServicosAExecutarList)
                .ThenInclude(s => s.Servico)
            .Include(o => o.ServicosAExecutarList)
                .ThenInclude(s => s.Mecanico)
            .Include(o => o.ApontamentosTempo)
                .ThenInclude(a => a.Mecanico)
            .Include(o => o.Pagamentos)
            .Include(o => o.MecanicoResponsavel)
            .Include(o => o.CanceladaPorUsuario)
            .Include(o => o.Contatos)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public Task<Configuracao?> ObterConfiguracaoAsync()
    {
        return _db.Configuracoes.AsNoTracking().FirstOrDefaultAsync();
    }

    public Task<List<Veiculo>> ListarVeiculosAtivosParaSelecaoAsync()
    {
        return _db.Veiculos
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Ativos()
            .Where(v => v.Cliente != null && v.Cliente.Ativo)
            .OrderBy(v => v.Cliente!.Nome)
            .ToListAsync();
    }

    public Task<List<Servico>> ListarServicosAtivosAsync()
    {
        return _db.Servicos
            .AsNoTracking()
            .Ativos()
            .OrderBy(s => s.Nome)
            .ToListAsync();
    }

    public Task<List<Peca>> ListarPecasAtivasAsync()
    {
        return _db.Pecas
            .AsNoTracking()
            .Ativos()
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<List<Peca>> BuscarPecasRapidoAsync(string? termo, int limite = 8)
    {
        var termoLimpo = termo?.Trim();
        if (string.IsNullOrEmpty(termoLimpo) || termoLimpo.Length < 2)
        {
            return new List<Peca>();
        }

        var t = termoLimpo.ToLower();
        return await _db.Pecas
            .AsNoTracking()
            .Ativos()
            .Where(p => p.Nome.ToLower().Contains(t) || p.Sku.ToLower().Contains(t))
            .OrderBy(p => p.Nome)
            .Take(limite)
            .ToListAsync();
    }

    public Task<List<Usuario>> ListarMecanicosAtivosAsync()
    {
        return _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Ativo && u.Perfil == PerfilUsuario.Mecanico)
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();
    }

    public async Task<List<OrdemServico>> ListarOrdensPainelMecanicoAsync(int usuarioId, PerfilUsuario perfil)
    {
        var statusAtivos = new[]
        {
            OrdemServicoStatus.Aprovada,
            OrdemServicoStatus.EmAndamento,
            OrdemServicoStatus.Pausada,
            OrdemServicoStatus.AguardandoPecas,
            OrdemServicoStatus.AguardandoCliente
        };

        var limiteConclusao = DateTime.UtcNow.AddDays(-2);
        var soMecanico = perfil == PerfilUsuario.Mecanico;

        var query = _db.OrdensServico
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.Veiculo)
            .Include(o => o.MecanicoResponsavel)
            .Include(o => o.ServicosAExecutarList).ThenInclude(s => s.Servico)
            .Include(o => o.PecasUtilizadas).ThenInclude(p => p.Peca)
            .Include(o => o.ApontamentosTempo)
            .Where(o => statusAtivos.Contains(o.Status)
                || (o.Status == OrdemServicoStatus.Concluida && o.DataConclusao >= limiteConclusao));

        if (soMecanico)
        {
            query = query.Where(o => o.MecanicoResponsavelId == usuarioId
                || (o.MecanicoResponsavelId == null && o.Status != OrdemServicoStatus.Concluida));
        }

        var carregadas = await query.OrderBy(o => o.Id).ToListAsync();
        var hoje = DateTimeExtensions.TodayBrasilia();

        return carregadas
            .Where(o => o.Status != OrdemServicoStatus.Concluida
                || (o.DataConclusao.HasValue && o.DataConclusao.Value.ToBrasiliaTime().Date == hoje))
            .ToList();
    }

    public Task<List<OrdemServico>> ListarAgendaMesAsync(int ano, int mes)
    {
        var inicio = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimExclusivo = inicio.AddMonths(1);

        return _db.OrdensServico
            .AsNoTracking()
            .Include(o => o.Veiculo)
                .ThenInclude(v => v!.Cliente)
            .Include(o => o.MecanicoResponsavel)
            .Where(o => o.DataPrevisaoEntrega >= inicio && o.DataPrevisaoEntrega < fimExclusivo)
            .OrderBy(o => o.DataPrevisaoEntrega)
            .ToListAsync();
    }

    public async Task<DashboardIndicadoresDto> ObterIndicadoresDashboardAsync(
        DateTime? filtroInicioUtc,
        DateTime? filtroFimUtc,
        DateTime hojeBrasilia)
    {
        var anosFrota = await _db.Veiculos
            .AsNoTracking()
            .Ativos()
            .Where(v => v.Ano > 1900 && v.Ano <= hojeBrasilia.Year)
            .Select(v => v.Ano)
            .ToListAsync();

        var totalVeiculos = anosFrota.Count;
        var idadeMediaFrota = totalVeiculos > 0
            ? (decimal)anosFrota.Average(ano => hojeBrasilia.Year - ano)
            : 0m;

        var osEmAndamento = await _db.OrdensServico
            .AsNoTracking()
            .Where(o => o.Status == OrdemServicoStatus.EmAndamento)
            .Select(o => new
            {
                MaoDeObra = o.ServicosAExecutarList
                    .Where(s => s.StatusItem != StatusItem.Recusado)
                    .Sum(s => (decimal?)(s.Quantidade * s.ValorCobrado)) ?? 0m,
                Pecas = o.PecasUtilizadas
                    .Where(p => p.StatusItem != StatusItem.Recusado)
                    .Sum(p => (decimal?)(p.Quantidade * p.ValorCobrado)) ?? 0m,
                Desconto = o.ValorDesconto
            })
            .ToListAsync();

        var totalEmAndamento = osEmAndamento.Count;
        var dinheiroNaMesa = osEmAndamento.Sum(o => (o.MaoDeObra + o.Pecas) - o.Desconto);

        var totalAtrasadas = await _db.OrdensServico
            .AsNoTracking()
            .CountAsync(o => (o.Status == OrdemServicoStatus.EmAndamento || o.Status == OrdemServicoStatus.AguardandoPecas)
                              && o.DataPrevisaoEntrega < hojeBrasilia);

        var queryMarcas = _db.OrdensServico
            .AsNoTracking()
            .Where(o => o.Veiculo != null && !string.IsNullOrEmpty(o.Veiculo.Marca));

        if (filtroInicioUtc.HasValue) queryMarcas = queryMarcas.Where(o => o.DataEmissao >= filtroInicioUtc.Value);
        if (filtroFimUtc.HasValue) queryMarcas = queryMarcas.Where(o => o.DataEmissao < filtroFimUtc.Value);

        var marcasAtendidas = await queryMarcas
            .Select(o => o.Veiculo!.Marca!)
            .ToListAsync();

        var topMarcas = marcasAtendidas
            .GroupBy(m => m.ToUpper().Trim())
            .Select(g => new MarcaRankingDto(
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(g.Key.ToLower()),
                g.Count()))
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        return new DashboardIndicadoresDto(
            totalVeiculos,
            idadeMediaFrota,
            totalEmAndamento,
            dinheiroNaMesa,
            totalAtrasadas,
            topMarcas);
    }
}
