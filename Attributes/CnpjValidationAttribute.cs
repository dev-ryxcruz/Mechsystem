using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace mechsystem.Attributes
{
    public class CnpjValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let [Required] handle this se necessário
            }

            string cnpj = value.ToString()!.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14 || !Regex.IsMatch(cnpj, @"^\d{14}$"))
            {
                return new ValidationResult("CNPJ deve conter 14 dígitos numéricos.");
            }

            bool allSame = true;
            for (int i = 1; i < 14; i++)
            {
                if (cnpj[i] != cnpj[0])
                {
                    allSame = false;
                    break;
                }
            }
            if (allSame) return new ValidationResult("CNPJ inválido.");

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            if (!cnpj.EndsWith(digito))
            {
                return new ValidationResult("CNPJ inválido.");
            }

            return ValidationResult.Success;
        }
    }
}
