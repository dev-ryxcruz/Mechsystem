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

        /// <summary>
        /// Cópia obrigatória do PrecoVenda da peça no momento da inserção na OS.
        /// Serve como referência de preço sugerido e não pode ser alterado após a inserção.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço Base (Sugerido)")]
        public decimal PrecoBase { get; set; }

        /// <summary>
        /// Valor efetivamente cobrado do cliente. Pode ser editado pelo operador.
        /// Se for menor que PrecoBase, constitui desconto e requer autorização de Administrador.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Cobrado (R$)")]
        public decimal ValorCobrado { get; set; }

        /// <summary>
        /// Snapshot do PrecoCusto no momento da inserção, para cálculo de margem real.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço de Custo (Snapshot)")]
        public decimal PrecoCustoSnapshot { get; set; }

        // ── Propriedades Calculadas ──────────────────────────────────────────────

        /// <summary>
        /// Subtotal da linha: Quantidade × ValorCobrado.
        /// </summary>
        [NotMapped]
        public decimal Subtotal => Quantidade * ValorCobrado;

        /// <summary>
        /// Indica se o valor cobrado está abaixo do preço base sugerido (desconto).
        /// Usado para bloqueio RBAC: perfil "Atendimento" não pode salvar se true.
        /// </summary>
        [NotMapped]
        public bool TemDescontoAbaixoDoMinimo => ValorCobrado < PrecoBase;
    }
}
