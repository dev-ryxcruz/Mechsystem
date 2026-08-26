using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace mechsystem.Attributes
{
    public class CpfValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let [Required] handle this se necessário
            }

            string cpf = value.ToString()!.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11 || !Regex.IsMatch(cpf, @"^\d{11}$"))
            {
                return new ValidationResult("CPF deve conter 11 dígitos.");
            }

            bool allSame = true;
            for (int i = 1; i < 11; i++)
            {
                if (cpf[i] != cpf[0])
                {
                    allSame = false;
                    break;
                }
            }
            if (allSame) return new ValidationResult("CPF inválido.");

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            if (!cpf.EndsWith(digito))
            {
                return new ValidationResult("CPF inválido.");
            }

            return ValidationResult.Success;
        }
    }
}
