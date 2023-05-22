using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace PeliculasApi.Tests
{
    public class UsuarioFalsoFiltro : IAsyncActionFilter // Se va aplicar y despúes de una acción
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, "ejemplo@hotmail.com"),
                new Claim(ClaimTypes.Name, "ejemplo@hotmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "9eecfade-df6f-46b6-bf31-22f518bbb7e9")
            }, "prueba"));

            await next();
        }
    }
}
