using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public enum OrigemPeca
    {
        [Display(Name = "Estoque")]
        Estoque = 0,

        [Display(Name = "Sob Demanda")]
        SobDemanda = 1,

        [Display(Name = "Cliente Trouxe")]
        ClienteTrouxe = 2
    }
}

