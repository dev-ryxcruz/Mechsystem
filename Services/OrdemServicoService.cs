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
            OrdemServicoStatus.EmAndamento,
            OrdemServicoStatus.Cancelada
        },
        [OrdemServicoStatus.EmAndamento] = new()
        {
            OrdemServicoStatus.Pausada,
            OrdemServicoStatus.AguardandoPecas,
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
        [OrdemServicoStatus.Concluida] = new(),
        [OrdemServicoStatus.Cancelada] = new()
    };

    public OrdemServicoService(AppDbContext db, EstoqueService estoqueService)
    {
        _db = db;
        _estoqueService = estoqueService;
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

    public async Task AprovarOSAsync(int osId, string? autorizadoPor, string? meioAutorizacao, int usuarioId)
    {
        var os = await _db.OrdensServico
            .Include(o => o.Vistoria)
            .Include(o => o.PecasUtilizadas)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status != OrdemServicoStatus.Orcamento)
        {
            throw new InvalidOperationException(
                $"Apenas O.S. em Orçamento podem ser aprovadas (status atual: '{os.Status}').");
        }

        ValidarTransicao(os.Status, OrdemServicoStatus.EmAndamento);

        var config = await _db.Configuracoes.FirstOrDefaultAsync();
        if (config != null && config.ObrigarVistoriaParaOS && os.Vistoria == null)
        {
            throw new InvalidOperationException(
                "Para aprovar este orçamento, as configurações da Oficina exigem a realização da Vistoria de Entrada. Realize a Vistoria na O.S. primeiro.");
        }

        os.Status = OrdemServicoStatus.EmAndamento;
        os.DataAutorizacao = DateTime.Now;
        os.AutorizadoPor = autorizadoPor;
        os.MeioAutorizacao = meioAutorizacao;

        if (!os.DataPrevisaoInicio.HasValue)
        {
            os.DataPrevisaoInicio = DateTime.Today;
        }

        await _db.SaveChangesAsync();

        if (os.PecasUtilizadas.Any())
        {
            await _estoqueService.BaixarPecasDaOS(os.Id, usuarioId);
        }
    }

    public async Task IniciarOuRetomarServicoAsync(int osId)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        if (os.Status == OrdemServicoStatus.Orcamento)
        {
            throw new InvalidOperationException(
                "A O.S. ainda está em Orçamento e precisa ser aprovada antes de iniciar a execução.");
        }

        if (os.Status == OrdemServicoStatus.EmAndamento)
        {
            if (!os.InicioUltimaExecucao.HasValue)
            {
                os.InicioUltimaExecucao = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            return;
        }

        ValidarTransicao(os.Status, OrdemServicoStatus.EmAndamento);

        os.Status = OrdemServicoStatus.EmAndamento;
        os.InicioUltimaExecucao = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task PausarServicoAsync(int osId)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.Pausada);

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.Pausada;
        await _db.SaveChangesAsync();
    }

    public async Task AguardarPecasAsync(int osId)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.AguardandoPecas);

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.AguardandoPecas;
        await _db.SaveChangesAsync();
    }

    public async Task ConcluirOSAsync(int osId)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.Concluida);

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.Concluida;
        await _db.SaveChangesAsync();
    }

    public async Task CancelarOSAsync(int osId, int usuarioId)
    {
        var os = await _db.OrdensServico
            .Include(o => o.PecasUtilizadas)
            .FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        var statusAnterior = os.Status;
        ValidarTransicao(statusAnterior, OrdemServicoStatus.Cancelada);

        if (statusAnterior != OrdemServicoStatus.Orcamento && os.PecasUtilizadas.Any())
        {
            await _estoqueService.EstornarPecasDaOS(os.Id, usuarioId);
        }

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.Cancelada;
        await _db.SaveChangesAsync();
    }

    private static void AcumularTempoExecucao(OrdemServico os)
    {
        if (os.InicioUltimaExecucao.HasValue)
        {
            var minutos = (int)Math.Round((DateTime.Now - os.InicioUltimaExecucao.Value).TotalMinutes);
            if (minutos > 0)
            {
                os.TempoGastoMinutos += minutos;
            }
            os.InicioUltimaExecucao = null;
        }
    }
}
