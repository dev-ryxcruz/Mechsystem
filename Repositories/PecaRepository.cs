using mechsystem.Data;
using mechsystem.Interfaces;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Repositories
{
    public class PecaRepository : IPecaRepository
    {
        private readonly AppDbContext _context;

        public PecaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Peca>> GetAllAsync(bool apenasAtivas = true)
        {
            var query = _context.Pecas.AsQueryable();
            if (apenasAtivas)
                query = query.Where(p => p.Ativo);
            return await query.OrderBy(p => p.Nome).ToListAsync();
        }

        public async Task<Peca?> GetByIdAsync(int id)
        {
            return await _context.Pecas.FindAsync(id);
        }

        public async Task<Peca?> GetBySkuAsync(string sku)
        {
            return await _context.Pecas.FirstOrDefaultAsync(p => p.Sku == sku);
        }

        public async Task<Peca> AddAsync(Peca peca)
        {
            _context.Pecas.Add(peca);
            await _context.SaveChangesAsync();
            return peca;
        }

        public async Task UpdateAsync(Peca peca)
        {
            _context.Entry(peca).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var peca = await _context.Pecas.FindAsync(id);
            if (peca != null)
            {
                peca.Ativo = false; // Soft-delete
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MovimentacaoEstoque>> GetHistoricoAsync(int pecaId)
        {
            return await _context.MovimentacoesEstoque
                .Include(m => m.Usuario)
                .Where(m => m.PecaId == pecaId)
                .OrderByDescending(m => m.DataHora)
                .ToListAsync();
        }

        // ─── Movimentações com Auditoria ─────────────────────────────────────────

        public async Task RegistrarEntradaAsync(int pecaId, int quantidade, decimal? novoPrecoCusto, string? referencia, int usuarioId)
        {
            var peca = await _context.Pecas.FindAsync(pecaId)
                ?? throw new InvalidOperationException($"Peça ID {pecaId} não encontrada.");

            peca.EstoqueAtual += quantidade;

            if (novoPrecoCusto.HasValue && novoPrecoCusto.Value > 0)
            {
                peca.PrecoCusto = novoPrecoCusto.Value;
            }

            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                PecaId = pecaId,
                Tipo = TipoMovimentacao.Entrada,
                Quantidade = quantidade,
                DataHora = DateTime.UtcNow,
                Referencia = referencia,
                UsuarioId = usuarioId
            });

            await _context.SaveChangesAsync();
        }

        public async Task RegistrarSaidaAsync(int pecaId, int quantidade, string? referencia, int usuarioId)
        {
            var peca = await _context.Pecas.FindAsync(pecaId)
                ?? throw new InvalidOperationException($"Peça ID {pecaId} não encontrada.");

            if (peca.EstoqueAtual < quantidade)
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente para '{peca.Nome}' (SKU: {peca.Sku}). " +
                    $"Disponível: {peca.EstoqueAtual}, Solicitado: {quantidade}.");
            }

            peca.EstoqueAtual -= quantidade;

            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                PecaId = pecaId,
                Tipo = TipoMovimentacao.Saida,
                Quantidade = -quantidade,
                DataHora = DateTime.UtcNow,
                Referencia = referencia,
                UsuarioId = usuarioId
            });

            await _context.SaveChangesAsync();
        }

        public async Task RegistrarAjusteAsync(int pecaId, int novaQuantidade, string motivo, int usuarioId)
        {
            var peca = await _context.Pecas.FindAsync(pecaId)
                ?? throw new InvalidOperationException($"Peça ID {pecaId} não encontrada.");

            var diferenca = novaQuantidade - peca.EstoqueAtual;
            peca.EstoqueAtual = novaQuantidade;

            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                PecaId = pecaId,
                Tipo = TipoMovimentacao.Ajuste,
                Quantidade = diferenca,
                DataHora = DateTime.UtcNow,
                Referencia = $"[AJUSTE] {motivo}",
                UsuarioId = usuarioId
            });

            await _context.SaveChangesAsync();
        }
    }
}
