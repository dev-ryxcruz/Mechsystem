using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class OrdemServico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        public int VeiculoId { get; set; }
        public Veiculo? Veiculo { get; set; }

        // Datas
        [Required]
        [Display(Name = "Data de Emissão")]
        public DateTime DataEmissao { get; set; } = DateTime.Now;

        [Display(Name = "Previsão de Início")]
        public DateTime? DataPrevisaoInicio { get; set; }

        [Required(ErrorMessage = "A previsão de entrega é obrigatória (CDC).")]
        [Display(Name = "Previsão de Entrega")]
        public DateTime DataPrevisaoEntrega { get; set; }

        // Valores
        [Display(Name = "Valor Mão de Obra")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo.")]
        public decimal ValorMaoDeObra { get; set; } = 0;

        /// <summary>
        /// Valor manual de peças. Mantido para retrocompatibilidade.
        /// REGRA DE GRACEFUL DEGRADATION: Se existirem peças vinculadas em PecasUtilizadas,
        /// este valor é IGNORADO e sobrescrito pela soma calculada das peças.
        /// </summary>
        [Display(Name = "Valor das Peças")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo.")]
        public decimal ValorPecas { get; set; } = 0;

        /// <summary>
        /// Valor efetivo de peças: se houver peças vinculadas, retorna a soma calculada;
        /// caso contrário, retorna o valor manual (fallback / graceful degradation).
        /// </summary>
        [NotMapped]
        public decimal ValorPecasEfetivo =>
            PecasUtilizadas != null && PecasUtilizadas.Any()
                ? PecasUtilizadas.Sum(p => p.Subtotal)
                : ValorPecas;

        /// <summary>
        /// Valor total da OS: Mão de Obra + Valor Efetivo de Peças.
        /// Usa ValorPecasEfetivo para aplicar a regra de graceful degradation.
        /// </summary>
        [Display(Name = "Valor Total")]
        public decimal ValorTotal => ValorMaoDeObra + ValorPecasEfetivo;

        // Descrições
        [Required(ErrorMessage = "O diagnóstico/problema relatado é obrigatório.")]
        [Display(Name = "Problema Relatado / Diagnóstico")]
        public string? DescricaoProblemaRelatado { get; set; }

        [Display(Name = "Serviços a Executar")]
        public string? ServicoAExecutar { get; set; }

        // Status
        [Required]
        public OrdemServicoStatus Status { get; set; } = OrdemServicoStatus.Orcamento;

        // Autorização (Obrigatório para virar contrato)
        [Display(Name = "Autorizado por (Assinatura/Nome)")]
        public string? AutorizadoPor { get; set; }

        [Display(Name = "Meio de Autorização")]
        public string? MeioAutorizacao { get; set; } // "Presencial", "WhatsApp", "Telefone"

        [Display(Name = "Data da Autorização")]
        public DateTime? DataAutorizacao { get; set; }

        [Display(Name = "Validade do Orçamento")]
        public DateTime GetValidadeOrcamento(int validadeDias) => DataEmissao.AddDays(validadeDias);

        // Vistoria de Entrada (1 para 1)
        public Vistoria? Vistoria { get; set; }

        // Peças utilizadas na OS
        public ICollection<OrdemServicoPeca> PecasUtilizadas { get; set; } = new List<OrdemServicoPeca>();

        // Comunicação com Cliente
        public string? TokenAcompanhamento { get; set; }
        public ICollection<ContatoOS> Contatos { get; set; } = new List<ContatoOS>();
    }
}
