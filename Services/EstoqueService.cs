using mechsystem.Data;
using mechsystem.Interfaces;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services
{
    public record ReservaEstoqueInfo(int PecaId, int EstoqueAtual, int QuantidadeReservada, int EstoqueDisponivel);

    public class EstoqueService
    {
        private readonly AppDbContext _context;
        private readonly IPecaRepository _pecaRepo;

        public EstoqueService(AppDbContext context, IPecaRepository pecaRepo)
        {
            _context = context;
            _pecaRepo = pecaRepo;
        }

        /// <summary>
        /// Calcula a quantidade reservada de uma peça por ordens de serviço ativas.
        /// Peças reservadas são aquelas de Origem = Estoque, aprovadas, ainda não aplicadas fisicamente e cuja O.S. não está cancelada/recusada/entregue.
        /// </summary>
        public async Task<int> ObterQuantidadeReservadaAsync(int pecaId, int? ordemServicoIdIgnorar = null)
        {
            var query = _context.OrdemServicoPecas
                .Where(op => op.PecaId == pecaId
                    && op.Origem == OrigemPeca.Estoque
                    && op.StatusItem == StatusItem.Aprovado
                    && !op.Aplicada
                    && op.OrdemServico != null
                    && op.OrdemServico.Status != OrdemServicoStatus.Cancelada
                    && op.OrdemServico.Status != OrdemServicoStatus.Recusada
                    && op.OrdemServico.Status != OrdemServicoStatus.Entregue);

            if (ordemServicoIdIgnorar.HasValue)
            {
                query = query.Where(op => op.OrdemServicoId != ordemServicoIdIgnorar.Value);
            }

            return await query.SumAsync(op => (int?)op.Quantidade) ?? 0;
        }

        /// <summary>
        /// Retorna a posição completa de estoque de uma peça: saldo atual, reserva e saldo disponível.
        /// </summary>
        public async Task<ReservaEstoqueInfo> ObterReservaInfoAsync(int pecaId, int? ordemServicoIdIgnorar = null)
        {
            var peca = await _context.Pecas.FindAsync(pecaId)
                ?? throw new InvalidOperationException($"Peça ID {pecaId} não encontrada.");

            var reservada = await ObterQuantidadeReservadaAsync(pecaId, ordemServicoIdIgnorar);
            var disponivel = Math.Max(0, peca.EstoqueAtual - reservada);

            return new ReservaEstoqueInfo(peca.Id, peca.EstoqueAtual, reservada, disponivel);
        }

        /// <summary>
        /// Valida se há saldo disponível (EstoqueAtual - QuantidadeReservada) suficiente para atender a quantidade solicitada.
        /// </summary>
        public async Task ValidarDisponibilidadeParaReservaAsync(int pecaId, int quantidadeDesejada, int? ordemServicoIdIgnorar = null)
        {
            var info = await ObterReservaInfoAsync(pecaId, ordemServicoIdIgnorar);
            var peca = await _context.Pecas.FindAsync(pecaId);
            var nomePeca = peca?.Nome ?? $"ID {pecaId}";
            var sku = peca?.Sku ?? "N/A";

            if (quantidadeDesejada > info.EstoqueDisponivel)
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente para reservar '{nomePeca}' (SKU: {sku}). Físico: {info.EstoqueAtual}, Reservado: {info.QuantidadeReservada}, Disponível: {info.EstoqueDisponivel}, Solicitado: {quantidadeDesejada}.");
            }
        }

        /// <summary>
        /// Valida a disponibilidade de todas as peças de estoque aprovadas em uma O.S. antes de confirmar a aprovação/reserva.
        /// </summary>
        public async Task ValidarDisponibilidadePecasAprovadasAsync(int ordemServicoId, IEnumerable<int>? pecasAprovadasIds = null)
        {
            var query = _context.OrdemServicoPecas
                .Include(op => op.Peca)
                .Where(op => op.OrdemServicoId == ordemServicoId && op.Origem == OrigemPeca.Estoque);

            if (pecasAprovadasIds != null)
            {
                var aprovadasSet = pecasAprovadasIds.ToHashSet();
                query = query.Where(op => aprovadasSet.Contains(op.Id));
            }
            else
            {
                query = query.Where(op => op.StatusItem != StatusItem.Recusado);
            }

            var itens = await query.ToListAsync();

            // Agrupa por PecaId para o caso de múltiplas linhas da mesma peça
            var agrupados = itens.GroupBy(i => i.PecaId);
            foreach (var grupo in agrupados)
            {
                var pecaId = grupo.Key;
                var totalNecessario = grupo.Sum(i => i.Quantidade);
                await ValidarDisponibilidadeParaReservaAsync(pecaId, totalNecessario, ordemServicoId);
            }
        }

        /// <summary>
        /// Converte a reserva em saída física no estoque ao marcar a peça como aplicada ao veículo.
        /// </summary>
        public async Task BaixarPecaAplicadaAsync(int ordemServicoPecaId, int usuarioId)
        {
            var item = await _context.OrdemServicoPecas
                .Include(op => op.Peca)
                .FirstOrDefaultAsync(op => op.Id == ordemServicoPecaId)
                ?? throw new InvalidOperationException($"Item de peça ID {ordemServicoPecaId} não encontrado.");

            if (item.Origem != OrigemPeca.Estoque)
            {
                return; // Peças sob demanda ou trazidas pelo cliente não movimentam o inventário local
            }

            var peca = item.Peca ?? await _context.Pecas.FindAsync(item.PecaId)
                ?? throw new InvalidOperationException($"Peça ID {item.PecaId} não encontrada.");

            if (peca.EstoqueAtual < item.Quantidade)
            {
                throw new InvalidOperationException(
                    $"Saldo físico insuficiente no momento da aplicação para '{peca.Nome}' (SKU: {peca.Sku}). Saldo atual: {peca.EstoqueAtual}, Solicitado: {item.Quantidade}.");
            }

            await _pecaRepo.RegistrarSaidaAsync(
                item.PecaId,
                item.Quantidade,
                $"Aplicação na OS #{item.OrdemServicoId.ToString("D4")}",
                usuarioId
            );
        }

        /// <summary>
        /// Estorna apenas peças que foram fisicamente aplicadas (baixadas) na O.S.
        /// Peças que estavam apenas reservadas (não aplicadas) têm suas reservas liberadas sem movimentar saldo físico.
        /// </summary>
        public async Task EstornarPecasCancelamentoOSAsync(int ordemServicoId, int usuarioId)
        {
            var pecasAplicadas = await _context.OrdemServicoPecas
                .Where(op => op.OrdemServicoId == ordemServicoId
                    && op.Origem == OrigemPeca.Estoque
                    && op.Aplicada)
                .ToListAsync();

            foreach (var item in pecasAplicadas)
            {
                await _pecaRepo.RegistrarEntradaAsync(
                    item.PecaId,
                    item.Quantidade,
                    null, // Não altera preço de custo médio em estorno
                    $"[ESTORNO] Cancelamento da OS #{ordemServicoId.ToString("D4")}",
                    usuarioId
                );
            }
        }

        /// <summary>
        /// Compatibilidade retroativa para rotinas antigas: baixa todas as peças da O.S.
        /// </summary>
        public async Task BaixarPecasDaOS(int ordemServicoId, int usuarioId)
        {
            var pecasOS = await _context.OrdemServicoPecas
                .Include(op => op.Peca)
                .Where(op => op.OrdemServicoId == ordemServicoId && op.Origem == OrigemPeca.Estoque)
                .ToListAsync();

            foreach (var item in pecasOS)
            {
                var peca = item.Peca ?? await _context.Pecas.FindAsync(item.PecaId)
                    ?? throw new InvalidOperationException($"Peça ID {item.PecaId} não encontrada.");

                if (peca.EstoqueAtual < item.Quantidade)
                {
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para '{peca.Nome}' (SKU: {peca.Sku}). Disponível: {peca.EstoqueAtual}, Solicitado: {item.Quantidade}.");
                }
            }

            foreach (var item in pecasOS)
            {
                await _pecaRepo.RegistrarSaidaAsync(
                    item.PecaId,
                    item.Quantidade,
                    $"Referente à OS #{ordemServicoId.ToString("D4")}",
                    usuarioId
                );
                item.Aplicada = true;
            }
        }

        /// <summary>
        /// Compatibilidade retroativa para rotinas antigas: estorna peças da O.S.
        /// </summary>
        public async Task EstornarPecasDaOS(int ordemServicoId, int usuarioId)
        {
            await EstornarPecasCancelamentoOSAsync(ordemServicoId, usuarioId);
        }

        public async Task BaixarPecaIndividualDaOSAsync(int ordemServicoId, int pecaId, int quantidade, int usuarioId)
        {
            await _pecaRepo.RegistrarSaidaAsync(
                pecaId,
                quantidade,
                $"Adição de peça na OS #{ordemServicoId.ToString("D4")}",
                usuarioId
            );
        }

        public async Task EstornarPecaIndividualDaOSAsync(int ordemServicoId, int pecaId, int quantidade, int usuarioId)
        {
            await _pecaRepo.RegistrarEntradaAsync(
                pecaId,
                quantidade,
                null,
                $"[ESTORNO] Remoção de peça da OS #{ordemServicoId.ToString("D4")}",
                usuarioId
            );
        }

        /// <summary>
        /// Retorna peças ativas com estoque abaixo ou igual ao estoque mínimo (Alerta de Ruptura).
        /// </summary>
        public async Task<List<Peca>> GetAlertasRupturaAsync()
        {
            return await _context.Pecas
                .Ativos()
                .Where(p => p.EstoqueAtual <= p.EstoqueMinimo)
                .OrderBy(p => p.EstoqueAtual)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula o Capital Imobilizado em Estoque de peças ativas: Σ(EstoqueAtual × PrecoCusto).
        /// </summary>
        public async Task<decimal> GetCapitalImobilizadoAsync()
        {
            return await _context.Pecas
                .Ativos()
                .SumAsync(p => p.EstoqueAtual * p.PrecoCusto);
        }

        /// <summary>
        /// Calcula a Margem de Lucro Real das peças nas OS concluídas/entregues em um período.
        /// </summary>
        public async Task<(decimal lucroReal, decimal receitaPecas)> GetMargemLucroRealAsync(DateTime inicio, DateTime fim)
        {
            var pecasOS = await _context.OrdemServicoPecas
                .Include(op => op.OrdemServico)
                .Where(op => op.OrdemServico != null
                    && (op.OrdemServico.Status == OrdemServicoStatus.Concluida || op.OrdemServico.Status == OrdemServicoStatus.Entregue)
                    && op.OrdemServico.DataConclusao >= inicio
                    && op.OrdemServico.DataConclusao < fim
                    && op.StatusItem == StatusItem.Aprovado)
                .ToListAsync();

            var receitaPecas = pecasOS.Sum(op => op.Quantidade * op.ValorCobrado);
            var custoPecas = pecasOS.Sum(op => op.Quantidade * op.PrecoCustoSnapshot);
            var lucroReal = receitaPecas - custoPecas;

            return (lucroReal, receitaPecas);
        }
    }
}
