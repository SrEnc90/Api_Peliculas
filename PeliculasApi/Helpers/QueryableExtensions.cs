using PeliculasApi.DTOs;

namespace PeliculasApi.Helpers
{
    public static class QueryableExtensions
    {
        /*
         * AL colocar this IQueryable<T> como parámetro sstamos extendiendo la funciones de IQueryable para que incluya este método.
         * y como es static para invocarlo solo debemos colocar IQueryable.Paginar(parámetros)
         */
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
        {
            return queryable
                .Skip((paginacionDTO.Pagina - 1) * paginacionDTO.CantidadRegistrosPorPagina)
                .Take(paginacionDTO.CantidadRegistrosPorPagina);
        }
    }
}
