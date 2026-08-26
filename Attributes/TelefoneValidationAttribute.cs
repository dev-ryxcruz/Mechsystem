using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace mechsystem.Attributes
{
    public class TelefoneValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let [Required] handle this se necessário
            }

            string telefone = value.ToString()!.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");

            if (telefone.Length < 10 || telefone.Length > 11 || !Regex.IsMatch(telefone, @"^\d+$"))
            {
                return new ValidationResult("Telefone inválido. Deve conter 10 ou 11 dígitos numéricos com DDD.");
            }

            return ValidationResult.Success;
        }
    }
}
