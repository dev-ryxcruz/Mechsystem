using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using mechsystem.Attributes;

namespace mechsystem.Models
{
    public class Cliente : IInativavel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [CpfValidation]
        [MaxLength(14, ErrorMessage = "O CPF não pode exceder 14 caracteres.")]
        public string? Cpf { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string? Email { get; set; }
        
        [TelefoneValidation]
        public string? Telefone { get; set; }
        
        public string? Endereco { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime? DataInativacao { get; set; }
        public int? InativadoPorUsuarioId { get; set; }
        public Usuario? InativadoPorUsuario { get; set; }

        [MaxLength(500)]
        public string? MotivoInativacao { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
