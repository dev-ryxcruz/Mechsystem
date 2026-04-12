using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public enum NivelCombustivel
    {
        [Display(Name = "Reserva")]
        Reserva = 1,
        
        [Display(Name = "1/4 Tanque")]
        UmQuarto = 2,
        
        [Display(Name = "Meio Tanque")]
        Meio = 3,
        
        [Display(Name = "3/4 Tanque")]
        TresQuartos = 4,
        
        [Display(Name = "Cheio")]
        Cheio = 5
    }

    public class Vistoria
    {
        [Key]
        public int Id { get; set; }

        public int OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }

        public VistoriaStatus Status { get; set; } = VistoriaStatus.Pendente;

        [Required(ErrorMessage = "O nível de combustível é obrigatório.")]
        [Display(Name = "Nível de Combustível")]
        public NivelCombustivel NivelCombustivel { get; set; } = NivelCombustivel.Reserva;

        [Display(Name = "Quilometragem (Entrada)")]
        [Required(ErrorMessage = "A KM é obrigatória.")]
        public int QuilometragemEntrada { get; set; }

        // Checklist Items
        public bool TemEstepe { get; set; }
        public bool TemMacaco { get; set; }
        public bool TemRadio { get; set; }
        public bool TemTriangulo { get; set; }
        public bool TemChaveRoda { get; set; }

        // Mapeamento de Avarias via Visual (JSON data)
        // Guardará um array do tipo [{X:10, Y:20, Desc: "Risco na porta"}]
        [Column(TypeName = "TEXT")] // O sqlite e sqlserver aceitam string
        public string? AvariasJson { get; set; }

        [Display(Name = "Observações Adicionais")]
        public string? Observacoes { get; set; }
        
        public DateTime DataVistoria { get; set; } = DateTime.Now;
    }

    public enum VistoriaStatus
    {
        [Display(Name = "Pendente")]
        Pendente = 0,
        
        [Display(Name = "Concluída")]
        Concluida = 1
    }
}
