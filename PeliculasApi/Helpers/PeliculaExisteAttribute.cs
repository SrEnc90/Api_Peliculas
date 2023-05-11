using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace PeliculasApi.Helpers
{
    //Debemos inicializarlo con ambito Scope en la clase Startup
    public class PeliculaExisteAttribute : Attribute, IAsyncResourceFilter //Vamos a encasuplar lógica que se repite mucho sobre si existe la película en el ReviewController
    {
        private readonly ApplicationDbContext dbContext;

        public PeliculaExisteAttribute(ApplicationDbContext DbContext)
        {
            dbContext = DbContext;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, 
            ResourceExecutionDelegate next)
        {
            var peliculaIdObject = context.HttpContext.Request.RouteValues["peliculaId"]; //peliculaId proviene de la ruta del ReviewController, lo estamos almacenado
            if(peliculaIdObject == null) { return; }

            var peliculaId = int.Parse(peliculaIdObject.ToString()); // Lo convertimos a int xq en el reviewController es de tipo int [Route("api/peliculas/{peliculaId:int}/reviews")]

            var peliculaExiste = await dbContext.Peliculas.AnyAsync(x => x.Id == peliculaId);
            if (!peliculaExiste) { context.Result = new NotFoundResult(); }
            else { await next(); }

        }
    }
}
