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
            return await _context.Clientes.Where(c => c.Ativo).ToListAsync();
        }

        public async Task<Cliente?> BuscarPorId(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task Salvar(Cliente cliente)
        {
            cliente.Cpf = NormalizarCpf(cliente.Cpf);

            if (await _context.Clientes.IgnoreQueryFilters().AnyAsync(c => c.Cpf == cliente.Cpf))
            {
                throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF.");
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Cliente cliente)
        {
            cliente.Cpf = NormalizarCpf(cliente.Cpf);

            if (await _context.Clientes.IgnoreQueryFilters().AnyAsync(c => c.Cpf == cliente.Cpf && c.Id != cliente.Id))
            {
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

        public async Task Deletar(int id)
        {
            var cliente = await BuscarPorId(id);
            if (cliente != null)
            {
                cliente.Ativo = false;
                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();
            }
        }
    }
}