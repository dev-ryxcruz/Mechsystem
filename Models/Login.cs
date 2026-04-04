using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    
    public class Login
    {
        [Required(ErrorMessage = "Usuário é obrigatório.")]
        public string? Username { get; set; }
        
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        public string? Password { get; set;}


    }
}