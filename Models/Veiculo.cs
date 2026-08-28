using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [RegularExpression(@"^[a-zA-Z]{3}-?\d[a-zA-Z0-9]\d{2}$", ErrorMessage = "A placa deve estar no formato AAA-1234 ou padrão Mercosul (AAA1A23).")]
        [MaxLength(10)]
        public string? Placa { get; set; }
        
        public string? Marca { get; set;}
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        
        [Range(1950, 2100, ErrorMessage = "O ano deve estar entre 1950 e 2100.")]
        public int Ano { get; set; }
        
        [Range(0, 9999999, ErrorMessage = "A quilometragem não pode ser negativa.")]
        public int Quilometragem { get; set; }

        // Chave Estrangeira (Foreign Key) para ligar ao Cliente
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }


    }
}