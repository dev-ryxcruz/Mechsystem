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
                query = query.Ativos();
            return await query.OrderBy(p => p.Nome).ToListAsync();
        }

        public async Task<IEnumerable<Peca>> GetInativasAsync()
        {
            return await _context.Pecas
                .Inativos()
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<(List<Peca> Itens, int TotalCount)> GetPaginadoAsync(bool inativas, string? termoBusca, int pagina, int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;

            var query = inativas
                ? _context.Pecas.Inativos()
                : _context.Pecas.Ativos();

            if (!string.IsNullOrWhiteSpace(termoBusca))
            {
                var termo = termoBusca.Trim().ToLower();
                query = query.Where(p =>
                    p.Sku.ToLower().Contains(termo) ||
                    p.Nome.ToLower().Contains(termo) ||
                    (p.Marca != null && p.Marca.ToLower().Contains(termo)));
            }

            var totalCount = await query.CountAsync();
            var itens = await query
                .OrderBy(p => p.Nome)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalCount);
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
            peca.Sku = peca.Sku.Trim().ToUpperInvariant();

            var existente = await _context.Pecas
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Sku.ToUpper() == peca.Sku);

            if (existente != null)
            {
                if (!existente.Ativo)
                {
                    throw new InvalidOperationException(
                        $"Já existe uma peça inativa com este SKU: {existente.Nome}. Reative-a em Estoque > Inativos.");
                }
                throw new InvalidOperationException("Já existe uma peça cadastrada com este código SKU.");
            }

            _context.Pecas.Add(peca);
            await _context.SaveChangesAsync();
            return peca;
        }

        public async Task UpdateAsync(Peca peca)
        {
            peca.Sku = peca.Sku.Trim().ToUpperInvariant();

            var existente = await _context.Pecas
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Sku.ToUpper() == peca.Sku && p.Id != peca.Id);

            if (existente != null)
            {
                if (!existente.Ativo)
                {
                    throw new InvalidOperationException(
                        $"Já existe uma peça inativa com este SKU: {existente.Nome}. Reative-a em Estoque > Inativos.");
                }
                throw new InvalidOperationException("Já existe outra peça cadastrada com este código SKU.");
            }

            var existingEntity = _context.ChangeTracker.Entries<Peca>()
                .FirstOrDefault(e => e.Entity.Id == peca.Id);
            if (existingEntity != null)
            {
                existingEntity.State = EntityState.Detached;
            }

            _context.Entry(peca).State = EntityState.Modified;
            await SaveWithConcurrencyHandlingAsync();
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
            if (quantidade <= 0)
            {
                throw new InvalidOperationException("A quantidade de entrada deve ser maior que zero.");
            }

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

            await SaveWithConcurrencyHandlingAsync();
        }

        public async Task RegistrarSaidaAsync(int pecaId, int quantidade, string? referencia, int usuarioId)
        {
            if (quantidade <= 0)
            {
                throw new InvalidOperationException("A quantidade de saída deve ser maior que zero.");
            }

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

            await SaveWithConcurrencyHandlingAsync();
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

            await SaveWithConcurrencyHandlingAsync();
        }

        private async Task SaveWithConcurrencyHandlingAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync();
                }

                throw new InvalidOperationException(
                    "Conflito de concorrência: os dados ou o estoque desta peça foram alterados simultaneamente por outro usuário. Atualize a página e tente novamente.",
                    ex);
            }
        }
    }
}
