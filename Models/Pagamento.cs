using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public enum FormaPagamento
    {
        [Display(Name = "Dinheiro")]
        Dinheiro = 0,

        [Display(Name = "Pix")]
        Pix = 1,

        [Display(Name = "Cartão de Débito")]
        Debito = 2,

        [Display(Name = "Cartão de Crédito")]
        Credito = 3,

        [Display(Name = "Boleto")]
        Boleto = 4,

        [Display(Name = "Outro")]
        Outro = 5
    }

    public class Pagamento
    {
        public int Id { get; set; }

        [Required]
        public int OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }

        [Required]
        [Display(Name = "Forma de Pagamento")]
        public FormaPagamento Forma { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
        [Display(Name = "Valor (R$)")]
        public decimal Valor { get; set; }

        [Required]
        [Display(Name = "Data do Pagamento")]
        public DateTime DataPagamento { get; set; } = DateTime.UtcNow;

        [Required]
        public int RegistradoPorUsuarioId { get; set; }
        public Usuario? RegistradoPorUsuario { get; set; }
    }
}

