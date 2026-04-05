using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [MaxLength(10)]
        public string? Placa { get; set; }
        public string? Marca { get; set;}
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        public int Ano { get; set; }
        public int Quilometragem { get; set; }

        // Chave Estrangeira (Foreign Key) para ligar ao Cliente
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }


    }
}