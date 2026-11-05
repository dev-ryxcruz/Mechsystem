using mechsystem.Models;
using mechsystem.Interfaces;
using Microsoft.EntityFrameworkCore;
using mechsystem.Data;

namespace mechsystem.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> Listar()
        {
            return await _context.Clientes.Ativos().OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task<List<Cliente>> ListarInativos()
        {
            return await _context.Clientes.Inativos().OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task<(List<Cliente> Itens, int TotalCount)> ListarPaginadoAsync(bool inativos, string? termoBusca, int pagina, int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;

            var query = inativos
                ? _context.Clientes.Inativos()
                : _context.Clientes.Ativos();

            if (!string.IsNullOrWhiteSpace(termoBusca))
            {
                var termo = termoBusca.Trim().ToLower();
                var digitos = new string(termoBusca.Where(char.IsDigit).ToArray());

                query = query.Where(c =>
                    c.Nome.ToLower().Contains(termo) ||
                    (!string.IsNullOrEmpty(digitos) && c.Cpf.Contains(digitos)) ||
                    (!string.IsNullOrEmpty(digitos) && c.Telefone != null && c.Telefone.Contains(digitos)) ||
                    (c.Email != null && c.Email.ToLower().Contains(termo)));
            }

            var totalCount = await query.CountAsync();
            var itens = await query
                .OrderBy(c => c.Nome)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return (itens, totalCount);
        }

        public async Task<Cliente?> BuscarPorId(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task Salvar(Cliente cliente)
        {
            cliente.Cpf = NormalizarCpf(cliente.Cpf);

            var existente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Cpf == cliente.Cpf);

            if (existente != null)
            {
                if (!existente.Ativo)
                {
                    throw new InvalidOperationException(
                        $"Já existe um cliente inativo com este CPF: {existente.Nome}. Reative-o em Clientes > Inativos.");
                }
                throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF.");
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Cliente cliente)
        {
            cliente.Cpf = NormalizarCpf(cliente.Cpf);

            var existente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Cpf == cliente.Cpf && c.Id != cliente.Id);

            if (existente != null)
            {
                if (!existente.Ativo)
                {
                    throw new InvalidOperationException(
                        $"Já existe um cliente inativo com este CPF: {existente.Nome}. Reative-o em Clientes > Inativos.");
                }
                throw new InvalidOperationException("Já existe outro cliente cadastrado com este CPF.");
            }

            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        private static string NormalizarCpf(string? cpf)
        {
            return string.IsNullOrEmpty(cpf)
                ? string.Empty
                : new string(cpf.Where(char.IsDigit).ToArray());
        }
    }
}
