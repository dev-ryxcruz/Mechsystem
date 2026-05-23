using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class OrdemServicoServico
    {
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }

        [Required]
        public int ServicoId { get; set; }
        public Servico? Servico { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; } = 1;

        /// <summary>
        /// Cópia obrigatória do ValorPadrao do serviço no momento da inserção.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço Base (Sugerido)")]
        public decimal PrecoBase { get; set; }

        /// <summary>
        /// Valor efetivamente cobrado pelo serviço, podendo ser alterado pelo operador.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Cobrado (R$)")]
        public decimal ValorCobrado { get; set; }

        /// <summary>
        /// Cópia do tempo estimado no momento da inserção (em minutos).
        /// </summary>
        [Display(Name = "Tempo Estimado (Minutos) - Snapshot")]
        public int? TempoEstimadoMinutosSnapshot { get; set; }

        // ── Propriedades Calculadas ──────────────────────────────────────────────

        /// <summary>
        /// Subtotal da linha: Quantidade × ValorCobrado.
        /// </summary>
        [NotMapped]
        public decimal Subtotal => Quantidade * ValorCobrado;

        /// <summary>
        /// Tempo total estimado desta linha (Quantidade x Tempo unitário).
        /// </summary>
        [NotMapped]
        public int TempoTotalLinhaMinutos => (TempoEstimadoMinutosSnapshot ?? 0) * Quantidade;
    }
}
