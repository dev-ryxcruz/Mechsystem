using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class Peca
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O código SKU é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O SKU não pode exceder 50 caracteres.")]
        [Display(Name = "Código (SKU)")]
        public string Sku { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome da peça é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
        [Display(Name = "Nome da Peça")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "A marca não pode exceder 100 caracteres.")]
        [Display(Name = "Marca / Fabricante")]
        public string? Marca { get; set; }

        [Required(ErrorMessage = "O preço de custo é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O preço de custo não pode ser negativo.")]
        [Display(Name = "Preço de Custo (R$)")]
        public decimal PrecoCusto { get; set; }

        [Required(ErrorMessage = "O preço de venda é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O preço de venda não pode ser negativo.")]
        [Display(Name = "Preço de Venda (R$)")]
        public decimal PrecoVenda { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
        [Display(Name = "Estoque Atual")]
        public int EstoqueAtual { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "O estoque mínimo não pode ser negativo.")]
        [Display(Name = "Estoque Mínimo")]
        public int EstoqueMinimo { get; set; } = 2;

        [MaxLength(200)]
        [Display(Name = "Localização Física")]
        public string? Localizacao { get; set; }

        public bool Ativo { get; set; } = true;

        // Navigation
        public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();

        // Computed
        [NotMapped]
        public bool AbaixoDoMinimo => EstoqueAtual <= EstoqueMinimo;

        [NotMapped]
        public decimal MargemLucro => PrecoVenda > 0 ? ((PrecoVenda - PrecoCusto) / PrecoVenda) * 100 : 0;
    }
}
