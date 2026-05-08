using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    /// <summary>
    /// Classe abstrata base para todos os tipos de relatórios do sistema.
    /// Demonstra: CLASSE ABSTRATA, MÉTODO ABSTRATO e HERANÇA.
    /// Cada relatório especializado DEVE implementar GerarResumo() e GetIcone().
    /// </summary>
    public abstract class Relatorio
    {
        public string Titulo { get; set; } = string.Empty;
        public DateTime DataGeracao { get; set; } = DateTime.Now;
        public string GeradoPor { get; set; } = "Sistema";

        /// <summary>
        /// Método abstrato — cada tipo de relatório gera seu próprio resumo.
        /// As classes filhas DEVEM sobrescrever (override) este método.
        /// </summary>
        public abstract string GerarResumo();

        /// <summary>
        /// Método abstrato — cada relatório define seu ícone Bootstrap Icons.
        /// </summary>
        public abstract string GetIcone();

        /// <summary>
        /// Método abstrato — retorna a cor temática do card (CSS).
        /// </summary>
        public abstract string GetCorTema();

        /// <summary>
        /// Método concreto compartilhado por todos os relatórios.
        /// Gera o cabeçalho padrão formatado.
        /// </summary>
        public string GetCabecalho()
        {
            return $"{Titulo} — Gerado em {DataGeracao:dd/MM/yyyy HH:mm} por {GeradoPor}";
        }

        /// <summary>
        /// Método concreto — retorna a data de geração formatada em pt-BR.
        /// </summary>
        public string GetDataFormatada()
        {
            return DataGeracao.ToString("dd/MM/yyyy 'às' HH:mm",
                new System.Globalization.CultureInfo("pt-BR"));
        }
    }
}
