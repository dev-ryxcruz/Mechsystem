using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class ApontamentoTempo
    {
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }

        public int? OrdemServicoServicoId { get; set; }
        public OrdemServicoServico? OrdemServicoServico { get; set; }

        [Required]
        public int MecanicoId { get; set; }
        public Usuario? Mecanico { get; set; }

        [Required]
        [Display(Name = "Início")]
        public DateTime Inicio { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fim")]
        public DateTime? Fim { get; set; }

        [NotMapped]
        public int DuracaoMinutos
        {
            get
            {
                var fimEfetivo = Fim ?? DateTime.UtcNow;
                var inicioUtc = DateTime.SpecifyKind(Inicio, DateTimeKind.Utc);
                var fimUtc = DateTime.SpecifyKind(fimEfetivo, DateTimeKind.Utc);
                var minutos = (int)Math.Round((fimUtc - inicioUtc).TotalMinutes);
                return minutos > 0 ? minutos : 0;
            }
        }
    }
}

