using mechsystem.Data;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services;

public class InativacaoService
{
    private readonly AppDbContext _db;

    public static readonly OrdemServicoStatus[] StatusEmAberto =
    [
        OrdemServicoStatus.Orcamento,
        OrdemServicoStatus.Aprovada,
        OrdemServicoStatus.EmAndamento,
        OrdemServicoStatus.Pausada,
        OrdemServicoStatus.AguardandoPecas,
        OrdemServicoStatus.AguardandoCliente
    ];

    public InativacaoService(AppDbContext db)
    {
        _db = db;
    }

    public static bool EhStatusEmAberto(OrdemServicoStatus status) =>
        StatusEmAberto.Contains(status);

    public async Task<int> ContarVeiculosAtivosDoClienteAsync(int clienteId)
    {
        return await _db.Veiculos
            .Ativos()
            .CountAsync(v => v.ClienteId == clienteId);
    }

    public async Task<int> InativarClienteAsync(int clienteId, string? motivo, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);
        var motivoLimpo = ValidarMotivo(motivo);
        int veiculosInativados = 0;

        await ExecutarEmTransacaoAsync(async () =>
        {
            var cliente = await _db.Clientes
                .Include(c => c.Veiculos)
                .FirstOrDefaultAsync(c => c.Id == clienteId)
                ?? throw new InvalidOperationException("Cliente não encontrado.");

            var osEmAberto = await _db.OrdensServico
                .CountAsync(o => o.Veiculo != null && o.Veiculo.ClienteId == clienteId && StatusEmAberto.Contains(o.Status));

            if (osEmAberto > 0)
            {
                throw new InvalidOperationException(
                    $"Não é possível inativar o cliente '{cliente.Nome}' pois existem {osEmAberto} Ordem(ns) de Serviço em aberto vinculadas aos seus veículos.");
            }

            var agora = DateTime.UtcNow;
            cliente.Ativo = false;
            cliente.DataInativacao = agora;
            cliente.InativadoPorUsuarioId = usuarioId;
            cliente.MotivoInativacao = motivoLimpo;

            var veiculosAtivos = cliente.Veiculos.Where(v => v.Ativo).ToList();
            veiculosInativados = veiculosAtivos.Count;

            foreach (var veiculo in veiculosAtivos)
            {
                veiculo.Ativo = false;
                veiculo.DataInativacao = agora;
                veiculo.InativadoPorUsuarioId = usuarioId;
                veiculo.MotivoInativacao = $"Inativado em cascata com o cliente: {motivoLimpo}";
            }

            await _db.SaveChangesAsync();
        });

        return veiculosInativados;
    }

    public async Task ReativarClienteAsync(int clienteId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId)
                ?? throw new InvalidOperationException("Cliente não encontrado.");

            // Reativar cliente NÃO reativa veículos
            cliente.Ativo = true;
            cliente.DataInativacao = null;
            cliente.InativadoPorUsuarioId = null;
            cliente.MotivoInativacao = null;

            await _db.SaveChangesAsync();
        });
    }

    public async Task InativarVeiculoAsync(int veiculoId, string? motivo, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);
        var motivoLimpo = ValidarMotivo(motivo);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var veiculo = await _db.Veiculos.FirstOrDefaultAsync(v => v.Id == veiculoId)
                ?? throw new InvalidOperationException("Veículo não encontrado.");

            var osEmAberto = await _db.OrdensServico
                .CountAsync(o => o.VeiculoId == veiculoId && StatusEmAberto.Contains(o.Status));

            if (osEmAberto > 0)
            {
                throw new InvalidOperationException(
                    $"Não é possível inativar o veículo '{veiculo.Placa}' pois possui {osEmAberto} Ordem(ns) de Serviço em aberto.");
            }

            veiculo.Ativo = false;
            veiculo.DataInativacao = DateTime.UtcNow;
            veiculo.InativadoPorUsuarioId = usuarioId;
            veiculo.MotivoInativacao = motivoLimpo;

            await _db.SaveChangesAsync();
        });
    }

    public async Task ReativarVeiculoAsync(int veiculoId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var veiculo = await _db.Veiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == veiculoId)
                ?? throw new InvalidOperationException("Veículo não encontrado.");

            if (veiculo.Cliente == null || !veiculo.Cliente.Ativo)
            {
                throw new InvalidOperationException(
                    $"Não é possível reativar o veículo '{veiculo.Placa}' pois o cliente proprietário ({veiculo.Cliente?.Nome ?? "não vinculado"}) está inativo. Reative o cliente primeiro.");
            }

            veiculo.Ativo = true;
            veiculo.DataInativacao = null;
            veiculo.InativadoPorUsuarioId = null;
            veiculo.MotivoInativacao = null;

            await _db.SaveChangesAsync();
        });
    }

    public async Task InativarPecaAsync(int pecaId, string? motivo, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);
        var motivoLimpo = ValidarMotivo(motivo);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var peca = await _db.Pecas.FirstOrDefaultAsync(p => p.Id == pecaId)
                ?? throw new InvalidOperationException("Peça não encontrada.");

            var emOsAberta = await _db.OrdemServicoPecas
                .AnyAsync(op => op.PecaId == pecaId && op.OrdemServico != null && StatusEmAberto.Contains(op.OrdemServico.Status));

            if (emOsAberta)
            {
                throw new InvalidOperationException(
                    $"Não é possível desativar a peça '{peca.Nome}' (SKU: {peca.Sku}) pois está vinculada a uma Ordem de Serviço em aberto.");
            }

            if (peca.EstoqueAtual > 0)
            {
                throw new InvalidOperationException(
                    $"Não é possível desativar a peça '{peca.Nome}' (SKU: {peca.Sku}) com saldo positivo ({peca.EstoqueAtual} un.). Zere o saldo por Ajuste antes de desativar.");
            }

            peca.Ativo = false;
            peca.DataInativacao = DateTime.UtcNow;
            peca.InativadoPorUsuarioId = usuarioId;
            peca.MotivoInativacao = motivoLimpo;

            await _db.SaveChangesAsync();
        });
    }

    public async Task ReativarPecaAsync(int pecaId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var peca = await _db.Pecas.FirstOrDefaultAsync(p => p.Id == pecaId)
                ?? throw new InvalidOperationException("Peça não encontrada.");

            peca.Ativo = true;
            peca.DataInativacao = null;
            peca.InativadoPorUsuarioId = null;
            peca.MotivoInativacao = null;

            await _db.SaveChangesAsync();
        });
    }

    public async Task InativarServicoAsync(int servicoId, string? motivo, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);
        var motivoLimpo = ValidarMotivo(motivo);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var servico = await _db.Servicos.FirstOrDefaultAsync(s => s.Id == servicoId)
                ?? throw new InvalidOperationException("Serviço não encontrado.");

            var emOsAberta = await _db.OrdemServicoServicos
                .AnyAsync(os => os.ServicoId == servicoId && os.OrdemServico != null && StatusEmAberto.Contains(os.OrdemServico.Status));

            if (emOsAberta)
            {
                throw new InvalidOperationException(
                    $"Não é possível inativar o serviço '{servico.Nome}' pois está vinculado a uma Ordem de Serviço em aberto.");
            }

            servico.Ativo = false;
            servico.DataInativacao = DateTime.UtcNow;
            servico.InativadoPorUsuarioId = usuarioId;
            servico.MotivoInativacao = motivoLimpo;

            await _db.SaveChangesAsync();
        });
    }

    public async Task ReativarServicoAsync(int servicoId, int usuarioId, PerfilUsuario? perfilUsuario = null)
    {
        await GarantirAdministradorAsync(usuarioId, perfilUsuario);

        await ExecutarEmTransacaoAsync(async () =>
        {
            var servico = await _db.Servicos.FirstOrDefaultAsync(s => s.Id == servicoId)
                ?? throw new InvalidOperationException("Serviço não encontrado.");

            servico.Ativo = true;
            servico.DataInativacao = null;
            servico.InativadoPorUsuarioId = null;
            servico.MotivoInativacao = null;

            await _db.SaveChangesAsync();
        });
    }

    private static string ValidarMotivo(string? motivo)
    {
        var limpo = motivo?.Trim() ?? string.Empty;
        if (limpo.Length < 5)
        {
            throw new InvalidOperationException("O motivo da inativação é obrigatório e deve ter pelo menos 5 caracteres.");
        }
        return limpo;
    }

    private async Task GarantirAdministradorAsync(int usuarioId, PerfilUsuario? perfilInformado)
    {
        if (usuarioId <= 0 && !perfilInformado.HasValue)
        {
            throw new UnauthorizedAccessException("Apenas Administradores possuem permissão para inativar ou reativar registros.");
        }

        PerfilUsuario? perfil = perfilInformado;
        if (!perfil.HasValue && usuarioId > 0)
        {
            perfil = await _db.Usuarios
                .AsNoTracking()
                .Where(u => u.Id == usuarioId && u.Ativo)
                .Select(u => (PerfilUsuario?)u.Perfil)
                .FirstOrDefaultAsync();
        }

        if (perfil != PerfilUsuario.Administrador)
        {
            throw new UnauthorizedAccessException("Apenas Administradores possuem permissão para inativar ou reativar registros.");
        }
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
                "Conflito de concorrência: o registro foi alterado simultaneamente por outro usuário. Atualize a página e tente novamente.",
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
}
