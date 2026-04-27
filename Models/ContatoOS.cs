using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public class ContatoOS
    {
        public int Id { get; set; }
        
        public int OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }
        
        [Display(Name = "Data do Contato")]
        public DateTime DataContato { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "O tipo de contato é obrigatório.")]
        public TipoContato Tipo { get; set; }
        
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [Display(Name = "Descrição / Observação")]
        public string Descricao { get; set; } = string.Empty;
        
        [Display(Name = "Registrado por")]
        public string RegistradoPor { get; set; } = string.Empty;
    }

    public enum TipoContato
    {
        [Display(Name = "Ligação")]
        Ligacao,
        
        [Display(Name = "WhatsApp")]
        WhatsApp,
        
        [Display(Name = "E-mail")]
        Email,
        
        [Display(Name = "Presencial")]
        Presencial,
        
        [Display(Name = "Outro")]
        Outro
    }
}
