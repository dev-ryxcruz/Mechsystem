using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public enum TipoMovimentacao
    {
        [Display(Name = "Entrada")]
        Entrada = 0,

        [Display(Name = "Saída")]
        Saida = 1,

        [Display(Name = "Ajuste")]
        Ajuste = 2
    }
}
