using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public enum StatusItem
    {
        [Display(Name = "Pendente")]
        Pendente = 0,

        [Display(Name = "Aprovado")]
        Aprovado = 1,

        [Display(Name = "Recusado")]
        Recusado = 2
    }
}

