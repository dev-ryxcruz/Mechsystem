namespace mechsystem.Models
{
    /// <summary>
    /// Relatório de Estoque — herda de Relatorio (classe abstrata).
    /// Demonstra: HERANÇA e SOBRESCRITA DE MÉTODOS (override).
    /// </summary>
    public class RelatorioEstoque : Relatorio
    {
        public int TotalPecasCadastradas { get; set; }
        public int PecasAtivas { get; set; }
        public int PecasAbaixoDoMinimo { get; set; }
        public decimal CapitalImobilizado { get; set; }
        public decimal MargemLucroMedia { get; set; }
        public int TotalMovimentacoes { get; set; }

        /// <summary>
        /// Sobrescrita (override) do método abstrato GerarResumo().
        /// Retorna resumo específico de Estoque.
        /// </summary>
        public override string GerarResumo()
        {
            var cultura = new System.Globalization.CultureInfo("pt-BR");
            var statusRuptura = PecasAbaixoDoMinimo > 0
                ? $"⚠ {PecasAbaixoDoMinimo} peça(s) em ruptura!"
                : "✅ Estoque saudável";

            return $"Peças cadastradas: {TotalPecasCadastradas} (Ativas: {PecasAtivas}) | " +
                   $"Capital Imobilizado: {CapitalImobilizado.ToString("C", cultura)} | " +
                   $"Margem Média: {MargemLucroMedia:N1}% | " +
                   $"Movimentações: {TotalMovimentacoes} | " +
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
