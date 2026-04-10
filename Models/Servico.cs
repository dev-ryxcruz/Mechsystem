using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class Servico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O valor padrão é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Padrão (R$)")]
        public decimal ValorPadrao { get; set; }

        [Display(Name = "Tempo Estimado (Minutos)")]
        [Range(0, 10000, ErrorMessage = "O tempo estimado deve ser um valor positivo.")]
        public int? TempoEstimadoMinutos { get; set; }
    }
}
