using System.ComponentModel.DataAnnotations;

namespace mechsystem.Models
{
    public interface IInativavel
    {
        bool Ativo { get; set; }
        DateTime? DataInativacao { get; set; }
        int? InativadoPorUsuarioId { get; set; }
        string? MotivoInativacao { get; set; }
    }

    public static class InativavelExtensions
    {
        public static IQueryable<T> Ativos<T>(this IQueryable<T> query) where T : class, IInativavel
            => query.Where(x => x.Ativo);

        public static IQueryable<T> Inativos<T>(this IQueryable<T> query) where T : class, IInativavel
            => query.Where(x => !x.Ativo);

        public static IEnumerable<T> Ativos<T>(this IEnumerable<T> source) where T : class, IInativavel
            => source.Where(x => x.Ativo);

        public static IEnumerable<T> Inativos<T>(this IEnumerable<T> source) where T : class, IInativavel
            => source.Where(x => !x.Ativo);
    }
}
