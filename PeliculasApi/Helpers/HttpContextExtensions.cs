using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace PeliculasApi.Helpers
{
    public static class HttpContextExtensions
    {
        /*
         * AL colocar this HttpContext como parámetros estamos extendiendo la funciones de HttpContext para que incluya este método.
         * y como es static para invocarlo solo debemos colocar HttpContext.InsertarParametrosPaginacion(parámetros)
         */
        public async static Task InsertarParametrosPaginacion<T>(this HttpContext httpContext,
            IQueryable<T> queryable, int cantidadRegistrosPorPagina)
        {
            double cantidad = await queryable.CountAsync();
            double cantidadPaginas = Math.Ceiling(cantidad / cantidadRegistrosPorPagina);
            httpContext.Response.Headers.Add("cantidadPaginas", cantidadPaginas.ToString());
        }
    }
}
