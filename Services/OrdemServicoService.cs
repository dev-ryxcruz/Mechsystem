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

    public async Task AprovarOSAsync(int osId, string? autorizadoPor, string? meioAutorizacao, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (perfil.HasValue && perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento)
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para aprovar ordens de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
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

            if (os.PecasUtilizadas.Any())
            {
                await _estoqueService.BaixarPecasDaOS(os.Id, usuarioId);
            }

            os.Status = OrdemServicoStatus.EmAndamento;
            os.DataAutorizacao = DateTime.UtcNow;
            os.AutorizadoPor = autorizadoPor;
            os.MeioAutorizacao = meioAutorizacao;

            if (!os.DataPrevisaoInicio.HasValue)
            {
                os.DataPrevisaoInicio = DateTime.UtcNow.Date;
            }

            await _db.SaveChangesAsync();
        });
    }

    public async Task IniciarOuRetomarServicoAsync(int osId, PerfilUsuario? perfilUsuario = null)
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
                os.InicioUltimaExecucao = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return;
        }

        ValidarTransicao(os.Status, OrdemServicoStatus.EmAndamento);

        os.Status = OrdemServicoStatus.EmAndamento;
        os.InicioUltimaExecucao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task PausarServicoAsync(int osId, PerfilUsuario? perfilUsuario = null)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.Pausada);

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.Pausada;
        await _db.SaveChangesAsync();
    }

    public async Task AguardarPecasAsync(int osId, PerfilUsuario? perfilUsuario = null)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.AguardandoPecas);

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.AguardandoPecas;
        await _db.SaveChangesAsync();
    }

    public async Task ConcluirOSAsync(int osId, PerfilUsuario? perfilUsuario = null)
    {
        var os = await _db.OrdensServico.FirstOrDefaultAsync(o => o.Id == osId)
            ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

        ValidarTransicao(os.Status, OrdemServicoStatus.Concluida);

        AcumularTempoExecucao(os);
        os.Status = OrdemServicoStatus.Concluida;
        os.DataConclusao = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task CancelarOSAsync(int osId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (perfil.HasValue && perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento)
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para cancelar ordens de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            var statusAnterior = os.Status;
            ValidarTransicao(statusAnterior, OrdemServicoStatus.Cancelada);

            if (EstaComEstoqueBaixado(statusAnterior) && os.PecasUtilizadas.Any())
            {
                await _estoqueService.EstornarPecasDaOS(os.Id, usuarioId);
            }

            AcumularTempoExecucao(os);
            os.Status = OrdemServicoStatus.Cancelada;
            await _db.SaveChangesAsync();
        });
    }

    public static bool EstaComEstoqueBaixado(OrdemServicoStatus status)
    {
        return status == OrdemServicoStatus.EmAndamento
            || status == OrdemServicoStatus.Pausada
            || status == OrdemServicoStatus.AguardandoPecas;
    }

    public async Task AdicionarPecaAsync(int osId, int pecaId, int quantidade, decimal? valorCobrado, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
        }

        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status == OrdemServicoStatus.Concluida || os.Status == OrdemServicoStatus.Cancelada)
            {
                throw new InvalidOperationException($"Não é permitido adicionar peças a uma O.S. com status '{os.Status}'.");
            }

            var peca = await _db.Pecas.FindAsync(pecaId)
                ?? throw new InvalidOperationException("Peça não encontrada.");

            decimal cobrado = valorCobrado ?? peca.PrecoVenda;
            if (cobrado < 0)
            {
                throw new InvalidOperationException("O valor cobrado não pode ser negativo.");
            }

            if (cobrado < peca.PrecoVenda && perfil != PerfilUsuario.Administrador)
            {
                throw new UnauthorizedAccessException("Apenas Administradores podem aplicar desconto abaixo do preço base sugerido.");
            }

            if (EstaComEstoqueBaixado(os.Status))
            {
                await _estoqueService.BaixarPecaIndividualDaOSAsync(os.Id, peca.Id, quantidade, usuarioId);
            }

            var existente = os.PecasUtilizadas.FirstOrDefault(p => p.PecaId == pecaId);
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
                    PrecoCustoSnapshot = peca.PrecoCusto
                };
                _db.OrdemServicoPecas.Add(osPeca);
                os.PecasUtilizadas.Add(osPeca);
            }

            os.ValorPecas = os.PecasUtilizadas.Sum(p => p.Subtotal);
            await _db.SaveChangesAsync();
        });
    }

    public async Task RemoverPecaAsync(int osId, int osPecaId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        var perfil = await ObterPerfilAsync(usuarioId, perfilUsuario);
        if (perfil.HasValue && perfil.Value != PerfilUsuario.Administrador && perfil.Value != PerfilUsuario.Atendimento)
        {
            throw new UnauthorizedAccessException("Apenas Administradores e Atendimento possuem permissão para remover peças da ordem de serviço.");
        }

        await ExecutarEmTransacaoAsync(async () =>
        {
            var os = await _db.OrdensServico
                .Include(o => o.PecasUtilizadas)
                .FirstOrDefaultAsync(o => o.Id == osId)
                ?? throw new InvalidOperationException("Ordem de Serviço não encontrada.");

            if (os.Status == OrdemServicoStatus.Concluida || os.Status == OrdemServicoStatus.Cancelada)
            {
                throw new InvalidOperationException($"Não é permitido remover peças de uma O.S. com status '{os.Status}'.");
            }

            var item = os.PecasUtilizadas.FirstOrDefault(p => p.Id == osPecaId)
                ?? throw new InvalidOperationException("Peça vinculada não encontrada nesta O.S.");

            if (EstaComEstoqueBaixado(os.Status))
            {
                await _estoqueService.EstornarPecaIndividualDaOSAsync(os.Id, item.PecaId, item.Quantidade, usuarioId);
            }

            os.PecasUtilizadas.Remove(item);
            _db.OrdemServicoPecas.Remove(item);

            os.ValorPecas = os.PecasUtilizadas.Any() ? os.PecasUtilizadas.Sum(p => p.Subtotal) : 0;
            await _db.SaveChangesAsync();
        });
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
            }
            _db.ChangeTracker.Clear();

            throw new InvalidOperationException(
                "Conflito de concorrência: o estoque ou a O.S. foi alterada simultaneamente por outro usuário. Atualize a página e tente novamente.",
                ex);
        }
        catch
        {
            if (transacao != null)
            {
                await transacao.RollbackAsync();
            }
            _db.ChangeTracker.Clear();
            throw;
        }
    }

    private static void AcumularTempoExecucao(OrdemServico os)
    {
        if (os.InicioUltimaExecucao.HasValue)
        {
            var inicioUtc = DateTime.SpecifyKind(os.InicioUltimaExecucao.Value, DateTimeKind.Utc);
            var minutos = (int)Math.Round((DateTime.UtcNow - inicioUtc).TotalMinutes);
            if (minutos > 0)
            {
                os.TempoGastoMinutos += minutos;
            }
            os.InicioUltimaExecucao = null;
        }
    }
}
