namespace mechsystem.Models
{
    /// <summary>
    /// Relatório Financeiro — herda de Relatorio (classe abstrata).
    /// Demonstra: HERANÇA e SOBRESCRITA DE MÉTODOS (override).
    /// </summary>
    public class RelatorioFinanceiro : Relatorio
    {
        public decimal ReceitaMaoDeObra { get; set; }
        public decimal ReceitaPecas { get; set; }
        public decimal ReceitaTotal { get; set; }
        public decimal LucroRealPecas { get; set; }
        public decimal CapitalImobilizado { get; set; }
        public int TotalOSPagas { get; set; }

        /// <summary>
        /// Proporção de receita vinda de mão de obra (%).
        /// </summary>
        public double PercentualMaoDeObra =>
            ReceitaTotal > 0 ? (double)(ReceitaMaoDeObra / ReceitaTotal) * 100 : 0;

        /// <summary>
        /// Proporção de receita vinda de peças (%).
        /// </summary>
        public double PercentualPecas =>
            ReceitaTotal > 0 ? (double)(ReceitaPecas / ReceitaTotal) * 100 : 0;

        /// <summary>
        /// Sobrescrita (override) do método abstrato GerarResumo().
        /// Retorna resumo específico do panorama financeiro.
        /// </summary>
        public override string GerarResumo()
        {
            var cultura = new System.Globalization.CultureInfo("pt-BR");
            return $"Receita Total: {ReceitaTotal.ToString("C", cultura)} | " +
                   $"Mão de Obra: {ReceitaMaoDeObra.ToString("C", cultura)} ({PercentualMaoDeObra:N1}%) | " +
                   $"Peças: {ReceitaPecas.ToString("C", cultura)} ({PercentualPecas:N1}%) | " +
                   $"Lucro Real (Peças): {LucroRealPecas.ToString("C", cultura)} | " +
                   $"OS Pagas: {TotalOSPagas}";
        }

        /// <summary>
        /// Sobrescrita (override) — ícone financeiro.
        /// </summary>
        public override string GetIcone() => "bi-currency-dollar";

        /// <summary>
        /// Sobrescrita (override) — cor temática do relatório financeiro.
        /// </summary>
        public override string GetCorTema() => "#1cc88a";
    }
}
