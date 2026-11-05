namespace mechsystem.Models
{
    /// <summary>
    /// Relatório de Estoque — herda de Relatorio (classe abstrata).
    /// Demonstra: HERANÇA e SOBRESCRITA DE MÉTODOS (override).
    /// Separa indicadores de Posição Atual (tempo real) de Movimentações no Período.
    /// </summary>
    public class RelatorioEstoque : Relatorio
    {
        // Posição Atual (tempo real)
        public int TotalPecasCadastradas { get; set; }
        public int PecasAtivas { get; set; }
        public int PecasInativas { get; set; }
        public int PecasAbaixoDoMinimo { get; set; }
        public decimal CapitalImobilizado { get; set; }
        public decimal MargemLucroMedia { get; set; }

        // No Período Selecionado
        public int TotalMovimentacoes { get; set; }
        public int MovimentacoesEntradaPeriodo { get; set; }
        public int MovimentacoesSaidaPeriodo { get; set; }
        public int MovimentacoesAjustePeriodo { get; set; }

        /// <summary>
        /// Sobrescrita (override) do método abstrato GerarResumo().
        /// Retorna resumo específico de Estoque separando posição atual e período.
        /// </summary>
        public override string GerarResumo()
        {
            var cultura = new System.Globalization.CultureInfo("pt-BR");
            var statusRuptura = PecasAbaixoDoMinimo > 0
                ? $"⚠ {PecasAbaixoDoMinimo} peça(s) ativa(s) abaixo do mínimo!"
                : "✅ Estoque saudável";

            return $"Posição Atual — Peças Ativas: {PecasAtivas} (Inativas: {PecasInativas}) | " +
                   $"Capital Imobilizado (Ativas): {CapitalImobilizado.ToString("C", cultura)} | " +
                   $"Margem Média (Ativas): {MargemLucroMedia:N1}% | " +
                   $"No Período — Movimentações: {TotalMovimentacoes} (Entradas: {MovimentacoesEntradaPeriodo}, Saídas: {MovimentacoesSaidaPeriodo}, Ajustes: {MovimentacoesAjustePeriodo}) | " +
                   $"Status: {statusRuptura}";
        }

        /// <summary>
        /// Sobrescrita (override) — ícone de Estoque.
        /// </summary>
        public override string GetIcone() => "bi-box-seam";

        /// <summary>
        /// Sobrescrita (override) — cor temática do relatório de estoque.
        /// </summary>
        public override string GetCorTema() => "#fd7e14";
    }
}
