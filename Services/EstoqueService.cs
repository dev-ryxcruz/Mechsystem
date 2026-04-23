using mechsystem.Data;
using mechsystem.Interfaces;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services
{
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
        /// Baixa automática do estoque quando a OS é Aprovada (→ Em Andamento).
        /// Percorre todas as PecasUtilizadas da OS e registra saída de cada uma.
        /// </summary>
        public async Task BaixarPecasDaOS(int ordemServicoId, int usuarioId)
        {
            var pecasOS = await _context.OrdemServicoPecas
                .Where(op => op.OrdemServicoId == ordemServicoId)
                .ToListAsync();

            foreach (var item in pecasOS)
            {
                await _pecaRepo.RegistrarSaidaAsync(
                    item.PecaId,
                    item.Quantidade,
                    $"Referente à OS #{ordemServicoId.ToString("D4")}",
                    usuarioId
                );
            }
        }

        /// <summary>
        /// Estorno automático quando a OS é Cancelada.
        /// Devolve ao estoque todas as peças que foram baixadas.
        /// </summary>
        public async Task EstornarPecasDaOS(int ordemServicoId, int usuarioId)
        {
            var pecasOS = await _context.OrdemServicoPecas
                .Where(op => op.OrdemServicoId == ordemServicoId)
                .ToListAsync();

            foreach (var item in pecasOS)
            {
                await _pecaRepo.RegistrarEntradaAsync(
                    item.PecaId,
                    item.Quantidade,
                    null, // Não atualiza preço de custo no estorno
                    $"[ESTORNO] Cancelamento da OS #{ordemServicoId.ToString("D4")}",
                    usuarioId
                );
            }
        }

        /// <summary>
        /// Retorna peças com estoque abaixo ou igual ao estoque mínimo (Alerta de Ruptura).
        /// </summary>
        public async Task<List<Peca>> GetAlertasRupturaAsync()
        {
            return await _context.Pecas
                .Where(p => p.Ativo && p.EstoqueAtual <= p.EstoqueMinimo)
                .OrderBy(p => p.EstoqueAtual)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula o Capital Imobilizado em Estoque: Σ(EstoqueAtual × PrecoCusto).
        /// </summary>
        public async Task<decimal> GetCapitalImobilizadoAsync()
        {
            return await _context.Pecas
                .Where(p => p.Ativo)
                .SumAsync(p => p.EstoqueAtual * p.PrecoCusto);
        }

        /// <summary>
        /// Calcula a Margem de Lucro Real das peças nas OS concluídas em um período.
        /// Σ(PrecoVenda - PrecoCustoSnapshot) * Quantidade para todas as peças das OS concluídas.
        /// </summary>
        public async Task<(decimal lucroReal, decimal receitaPecas)> GetMargemLucroRealAsync(DateTime inicio, DateTime fim)
        {
            var pecasOS = await _context.OrdemServicoPecas
                .Include(op => op.OrdemServico)
                .Where(op => op.OrdemServico != null
                    && op.OrdemServico.Status == OrdemServicoStatus.Concluida
                    && op.OrdemServico.DataAutorizacao >= inicio
                    && op.OrdemServico.DataAutorizacao <= fim)
                .ToListAsync();

            var receitaPecas = pecasOS.Sum(op => op.Quantidade * op.ValorCobrado);
            var custoPecas = pecasOS.Sum(op => op.Quantidade * op.PrecoCustoSnapshot);
            var lucroReal = receitaPecas - custoPecas;

            return (lucroReal, receitaPecas);
        }
    }
}
