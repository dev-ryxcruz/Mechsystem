using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public class MovimentacaoEstoque
    {
        public int Id { get; set; }

        [Required]
        public int PecaId { get; set; }
        public Peca? Peca { get; set; }

        [Required]
        [Display(Name = "Tipo")]
        public TipoMovimentacao Tipo { get; set; }

        [Required]
        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }

        [Display(Name = "Data/Hora")]
        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        [MaxLength(300)]
        [Display(Name = "Referência / Motivo")]
        public string? Referencia { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
