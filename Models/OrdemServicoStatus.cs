using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public enum OrdemServicoStatus
    {
        [Display(Name = "Orçamento")]
        Orcamento = 0,

        [Display(Name = "Aguardando Peças")]
        AguardandoPecas = 1,

        [Display(Name = "Em Andamento")]
        EmAndamento = 2,

        [Display(Name = "Concluída")]
        Concluida = 3,

        [Display(Name = "Cancelada")]
        Cancelada = 4,

        [Display(Name = "Pausada")]
        Pausada = 5,

        [Display(Name = "Aprovada")]
        Aprovada = 6,

        [Display(Name = "Aguardando Cliente")]
        AguardandoCliente = 7,

        [Display(Name = "Entregue")]
        Entregue = 8,

        [Display(Name = "Recusada")]
        Recusada = 9
    }
}
