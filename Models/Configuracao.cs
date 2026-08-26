using System.ComponentModel.DataAnnotations;
using mechsystem.Attributes;

namespace mechsystem.Models
{
    public class Configuracao
    {
        public int Id { get; set; }

        // ─── Informações da Oficina ──────────────────────────────────────────────
        [Required(ErrorMessage = "O nome da oficina é obrigatório.")]
        [MaxLength(150)]
        public string NomeFantasia { get; set; } = "MechSystem Auto Center";

        [MaxLength(20)]
        public string? Cnpj { get; set; }

        [MaxLength(20)]
        [TelefoneValidation]
        public string? Telefone { get; set; }

        [MaxLength(20)]
        [TelefoneValidation]
        public string? WhatsApp { get; set; }

        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? EnderecoCompleto { get; set; }

        [MaxLength(500)]
        public string? MensagemRodape { get; set; }

        // ─── Regras do Sistema ───────────────────────────────────────────────────
        [Required]
        [Range(1, 365, ErrorMessage = "A validade deve ser entre 1 e 365 dias.")]
        public int ValidadeOrcamentoDias { get; set; } = 10;

        [Required]
        [Range(1, 3650, ErrorMessage = "A garantia deve ser entre 1 e 3650 dias.")]
        public int GarantiaPadraoDias { get; set; } = 90;

        public bool ObrigarVistoriaParaOS { get; set; } = false;

        // ─── Financeiro ──────────────────────────────────────────────────────────
        [MaxLength(10)]
        public string SimboloMoeda { get; set; } = "R$";

        [Range(0, 100, ErrorMessage = "A taxa de mão de obra deve ser entre 0% e 100%.")]
        public decimal TaxaMaoDeObra { get; set; } = 0m;
    }
}