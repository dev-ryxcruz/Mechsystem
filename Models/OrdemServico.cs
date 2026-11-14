using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mechsystem.Models
{
    public class OrdemServico : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        public int VeiculoId { get; set; }
        public Veiculo? Veiculo { get; set; }

        // Quilometragem e Combustível (registrados na O.S.)
        [Required(ErrorMessage = "A quilometragem de entrada é obrigatória.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        [Display(Name = "KM de Entrada")]
        public int KmEntrada { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        [Display(Name = "KM de Saída")]
        public int? KmSaida { get; set; }

        [Required(ErrorMessage = "O nível de combustível de entrada é obrigatório.")]
        [Display(Name = "Nível de Combustível (Entrada)")]
        public NivelCombustivel NivelCombustivelEntrada { get; set; } = NivelCombustivel.Reserva;

        // Datas
        [Required]
        [Display(Name = "Data de Emissão")]
        public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "A previsão de entrega é obrigatória (CDC).")]
        [Display(Name = "Previsão de Entrega")]
        public DateTime DataPrevisaoEntrega { get; set; }

        [Required]
        [Display(Name = "Validade do Orçamento Até")]
        public DateTime ValidadeOrcamentoAte { get; set; } = DateTime.UtcNow.AddDays(10);

        [Display(Name = "Garantia (Dias)")]
        public int? GarantiaDias { get; set; }

        [Display(Name = "Data da Entrega")]
        public DateTime? DataEntrega { get; set; }

        // Valores (fonte única = soma das linhas não recusadas)
        /// <summary>
        /// Valor efetivo da mão de obra: em orçamento soma linhas não recusadas; após aprovação soma apenas itens aprovados.
        /// </summary>
        [NotMapped]
        public decimal ValorMaoDeObraEfetivo =>
            ServicosAExecutarList != null
                ? (Status == OrdemServicoStatus.Orcamento
                    ? ServicosAExecutarList.Where(s => s.StatusItem != StatusItem.Recusado).Sum(s => s.Subtotal)
                    : ServicosAExecutarList.Where(s => s.StatusItem == StatusItem.Aprovado).Sum(s => s.Subtotal))
                : 0m;

        /// <summary>
        /// Valor efetivo de peças: em orçamento soma linhas não recusadas; após aprovação soma apenas itens aprovados.
        /// </summary>
        [NotMapped]
        public decimal ValorPecasEfetivo =>
            PecasUtilizadas != null
                ? (Status == OrdemServicoStatus.Orcamento
                    ? PecasUtilizadas.Where(p => p.StatusItem != StatusItem.Recusado).Sum(p => p.Subtotal)
                    : PecasUtilizadas.Where(p => p.StatusItem == StatusItem.Aprovado).Sum(p => p.Subtotal))
                : 0m;

        /// <summary>
        /// Valor total de aditivos ainda pendentes de aprovação pelo cliente.
        /// </summary>
        [NotMapped]
        public decimal ValorTotalAditivosPendentes =>
            (ServicosAExecutarList?.Where(s => s.StatusItem == StatusItem.Pendente && s.EhAditivo).Sum(s => s.Subtotal) ?? 0m) +
            (PecasUtilizadas?.Where(p => p.StatusItem == StatusItem.Pendente && p.EhAditivo).Sum(p => p.Subtotal) ?? 0m);

        /// <summary>
        /// Valor de desconto a ser aplicado diretamente na Ordem de Serviço.
        /// </summary>
        [Display(Name = "Desconto (R$)")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo.")]
        public decimal ValorDesconto { get; set; } = 0;

        /// <summary>
        /// Valor total da OS: Mão de Obra Efetiva + Peças Efetivas - Desconto.
        /// </summary>
        [NotMapped]
        [Display(Name = "Valor Total")]
        public decimal ValorTotal => (ValorMaoDeObraEfetivo + ValorPecasEfetivo) - ValorDesconto;

        /// <summary>
        /// Tempo total estimado para realizar todos os serviços: em orçamento soma linhas não recusadas; após aprovação soma apenas serviços aprovados.
        /// </summary>
        [NotMapped]
        [Display(Name = "Tempo Total Estimado (Minutos)")]
        public int TempoTotalEstimadoMinutos =>
            ServicosAExecutarList != null
                ? (Status == OrdemServicoStatus.Orcamento
                    ? ServicosAExecutarList.Where(s => s.StatusItem != StatusItem.Recusado).Sum(s => s.TempoTotalLinhaMinutos)
                    : ServicosAExecutarList.Where(s => s.StatusItem == StatusItem.Aprovado).Sum(s => s.TempoTotalLinhaMinutos))
                : 0;

        /// <summary>
        /// Tempo real total acumulado a partir dos apontamentos de tempo (em minutos).
        /// </summary>
        [NotMapped]
        [Display(Name = "Tempo Real Total (Minutos)")]
        public int TempoRealTotalMinutos =>
            ApontamentosTempo != null
                ? (int)Math.Round(ApontamentosTempo.Sum(a => ((a.Fim ?? DateTime.UtcNow) - a.Inicio).TotalMinutes))
                : 0;

        // Descrições (separação entre queixa do cliente e diagnóstico técnico)
        [Required(ErrorMessage = "A queixa do cliente é obrigatória.")]
        [Display(Name = "Queixa do Cliente")]
        public string QueixaCliente { get; set; } = string.Empty;

        [Display(Name = "Diagnóstico Técnico")]
        public string? Diagnostico { get; set; }

        // Responsável técnico
        [Display(Name = "Mecânico Responsável")]
        public int? MecanicoResponsavelId { get; set; }
        public Usuario? MecanicoResponsavel { get; set; }

        // Status
        [Required]
        public OrdemServicoStatus Status { get; set; } = OrdemServicoStatus.Orcamento;

        // Autorização (Obrigatório para virar contrato)
        [Display(Name = "Autorizado por (Assinatura/Nome)")]
        public string? AutorizadoPor { get; set; }

        [Display(Name = "Canal de Autorização")]
        public TipoContato? TipoContatoAutorizacao { get; set; }

        [Display(Name = "Aprovado por (Usuário)")]
        public int? AprovadoPorUsuarioId { get; set; }
        public Usuario? AprovadoPorUsuario { get; set; }

        [Display(Name = "Data da Autorização")]
        public DateTime? DataAutorizacao { get; set; }

        [Display(Name = "Data da Conclusão")]
        public DateTime? DataConclusao { get; set; }

        [Display(Name = "Checklist de Conclusão (JSON)")]
        [Column(TypeName = "jsonb")]
        public string? ChecklistConclusaoJson { get; set; }

        [MaxLength(500)]
        [Display(Name = "Motivo do Cancelamento")]
        public string? MotivoCancelamento { get; set; }

        [Display(Name = "Data do Cancelamento")]
        public DateTime? DataCancelamento { get; set; }

        public int? CanceladaPorUsuarioId { get; set; }
        public Usuario? CanceladaPorUsuario { get; set; }

        [Display(Name = "Validade do Orçamento")]
        public DateTime GetValidadeOrcamento(int validadeDias) =>
            ValidadeOrcamentoAte != default ? ValidadeOrcamentoAte : DataEmissao.AddDays(validadeDias);

        // Vistoria de Entrada (1 para 1)
        public Vistoria? Vistoria { get; set; }

        // Peças utilizadas na OS
        public ICollection<OrdemServicoPeca> PecasUtilizadas { get; set; } = new List<OrdemServicoPeca>();

        // Serviços listados na OS
        public ICollection<OrdemServicoServico> ServicosAExecutarList { get; set; } = new List<OrdemServicoServico>();

        // Apontamentos de tempo
        public ICollection<ApontamentoTempo> ApontamentosTempo { get; set; } = new List<ApontamentoTempo>();

        // Pagamentos registrados
        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();

        // Comunicação com Cliente
        public string? TokenAcompanhamento { get; set; } = Guid.NewGuid().ToString("N");
        public ICollection<ContatoOS> Contatos { get; set; } = new List<ContatoOS>();

        private static readonly Dictionary<OrdemServicoStatus, HashSet<OrdemServicoStatus>> TransicoesPermitidas = new()
        {
            [OrdemServicoStatus.Orcamento] = new()
            {
                OrdemServicoStatus.Aprovada,
                OrdemServicoStatus.Recusada,
                OrdemServicoStatus.Cancelada
            },
            [OrdemServicoStatus.Aprovada] = new()
            {
                OrdemServicoStatus.EmAndamento,
                OrdemServicoStatus.Cancelada
            },
            [OrdemServicoStatus.EmAndamento] = new()
            {
                OrdemServicoStatus.Pausada,
                OrdemServicoStatus.AguardandoPecas,
                OrdemServicoStatus.AguardandoCliente,
                OrdemServicoStatus.Concluida,
                OrdemServicoStatus.Cancelada
            },
            [OrdemServicoStatus.Pausada] = new()
            {
                OrdemServicoStatus.EmAndamento,
                OrdemServicoStatus.Cancelada
            },
            [OrdemServicoStatus.AguardandoPecas] = new()
            {
                OrdemServicoStatus.EmAndamento,
                OrdemServicoStatus.Cancelada
            },
            [OrdemServicoStatus.AguardandoCliente] = new()
            {
                OrdemServicoStatus.EmAndamento,
                OrdemServicoStatus.Cancelada
            },
            [OrdemServicoStatus.Concluida] = new()
            {
                OrdemServicoStatus.Entregue
            },
            [OrdemServicoStatus.Entregue] = new(),
            [OrdemServicoStatus.Recusada] = new(),
            [OrdemServicoStatus.Cancelada] = new()
        };

        public static bool PodeTransicionar(OrdemServicoStatus origem, OrdemServicoStatus destino) =>
            TransicoesPermitidas.TryGetValue(origem, out var destinos) && destinos.Contains(destino);

        public static void ValidarTransicao(OrdemServicoStatus origem, OrdemServicoStatus destino)
        {
            if (!PodeTransicionar(origem, destino))
            {
                throw new InvalidOperationException(
                    $"Transição de status inválida: não é permitido alterar a O.S. de '{origem}' para '{destino}'.");
            }
        }

        public void ValidarTransicaoPara(OrdemServicoStatus destino) => ValidarTransicao(Status, destino);

        public void FecharApontamentosAbertos(DateTime? instanteUtc = null)
        {
            if (ApontamentosTempo == null) return;
            var agora = instanteUtc ?? DateTime.UtcNow;
            foreach (var apontamento in ApontamentosTempo.Where(a => !a.Fim.HasValue))
            {
                apontamento.Fim = agora;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataPrevisaoEntrega.Date < DataEmissao.Date)
            {
                yield return new ValidationResult(
                    "A data de previsão de entrega não pode ser anterior à data de emissão.",
                    new[] { nameof(DataPrevisaoEntrega) }
                );
            }
        }
    }

    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo BrasiliaTimeZone = ObterTimeZoneBrasilia();

        private static TimeZoneInfo ObterTimeZoneBrasilia()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                }
                catch
                {
                    return TimeZoneInfo.Local;
                }
            }
        }

        public static DateTime ToBrasiliaTime(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
            {
                return TimeZoneInfo.ConvertTime(dateTime, BrasiliaTimeZone);
            }

            var utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, BrasiliaTimeZone);
        }

        public static DateTime? ToBrasiliaTime(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToBrasiliaTime() : null;
        }

        public static DateTime NowBrasilia() => DateTime.UtcNow.ToBrasiliaTime();

        public static DateTime TodayBrasilia() => DateTime.UtcNow.ToBrasiliaTime().Date;
    }

    public static class DomainErrorHelper
    {
        public static string ObterMensagemSegura(
            Exception ex,
            Microsoft.Extensions.Logging.ILogger? logger,
            string mensagemPadrao = "Ocorreu um erro interno ao processar a solicitação. Tente novamente.")
        {
            if (ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                return ex.Message;
            }

            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx &&
                dbEx.InnerException?.Message.Contains("ix_pecas_sku", StringComparison.OrdinalIgnoreCase) == true)
            {
                return "Já existe uma peça cadastrada com este código SKU.";
            }

            logger?.LogError(ex, "Erro inesperado: {MensagemPadrao}", mensagemPadrao);
            return mensagemPadrao;
        }
    }
}
