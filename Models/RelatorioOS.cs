namespace mechsystem.Models
{
    /// <summary>
    /// Relatório de Ordens de Serviço — herda de Relatorio (classe abstrata).
    /// Demonstra: HERANÇA e SOBRESCRITA DE MÉTODOS (override).
    /// </summary>
    public class RelatorioOS : Relatorio
    {
        public int TotalOS { get; set; }
        public int TotalConcluidas { get; set; }
        public int TotalCanceladas { get; set; }
        public int TotalEmAndamento { get; set; }
        public int TotalOrcamentos { get; set; }
        public decimal FaturamentoTotal { get; set; }
        public decimal TicketMedio { get; set; }
        public double TaxaConversao { get; set; }

        /// <summary>
        /// Sobrescrita (override) do método abstrato GerarResumo().
        /// Retorna resumo específico de Ordens de Serviço.
        /// </summary>
        public override string GerarResumo()
        {
            var cultura = new System.Globalization.CultureInfo("pt-BR");
            return $"Total de OS: {TotalOS} | " +
                   $"Concluídas: {TotalConcluidas} | " +
                   $"Canceladas: {TotalCanceladas} | " +
                   $"Em Andamento: {TotalEmAndamento} | " +
                   $"Faturamento: {FaturamentoTotal.ToString("C", cultura)} | " +
                   $"Ticket Médio: {TicketMedio.ToString("C", cultura)} | " +
                   $"Conversão: {TaxaConversao:N1}%";
        }

        /// <summary>
        /// Sobrescrita (override) — ícone de OS.
        /// </summary>
        public override string GetIcone() => "bi-clipboard-check";

        /// <summary>
        /// Sobrescrita (override) — cor temática do relatório de OS.
        /// </summary>
        public override string GetCorTema() => "#4e73df";
    }
}
