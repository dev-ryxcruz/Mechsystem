using mechsystem.Data;
using mechsystem.Models;
using Microsoft.EntityFrameworkCore;

namespace mechsystem.Services
{
    public class ConfiguracaoService
    {
        private readonly AppDbContext _context;

        public ConfiguracaoService(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<Configuracao> GetConfiguracaoAsync() // Cria configuração se não existir
        {
            var config = await _context.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);
            
            if (config == null)
            {
                config = new Configuracao { Id = 1 }; // Inicia como Id 1
                _context.Configuracoes.Add(config);
                await _context.SaveChangesAsync();
            }
            
            return config;
        }

        public async Task SalvarConfiguracaoAsync(Configuracao configuracao)
        {
            _context.Configuracoes.Update(configuracao);
            await _context.SaveChangesAsync();
        }
    }
}