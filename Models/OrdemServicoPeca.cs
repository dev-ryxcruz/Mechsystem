using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class OrdemServicoPeca
    {
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }

        [Required]
        public int PecaId { get; set; }
        public Peca? Peca { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço Unitário (Venda)")]
        public decimal PrecoUnitarioVenda { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço de Custo (Snapshot)")]
        public decimal PrecoCustoSnapshot { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantidade * PrecoUnitarioVenda;
    }
}
