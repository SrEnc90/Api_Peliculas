using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using PeliculasApi.Migrations;
using System.Security.Claims;

namespace PeliculasApi.Controllers
{
    [Route("api/peliculas/{peliculaId:int}/reviews")] //los reviews van hacer hijos de la clase película,Ejemplo de ver los reviews de la película con id=2 api/peliculas/2/reviews
    [ServiceFilter(typeof(PeliculaExisteAttribute))]
    [ApiController]
    public class ReviewController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ReviewController(ApplicationDbContext context, IMapper mapper)
            :base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReviewDTO>>> Get(int peliculaId,
            [FromQuery] PaginacionDTO paginacionDTO)
        {
            //Encapsulando lógica que se repite en el helper PeliculaExisteAttribute
            //var existePelicula = await context.Peliculas.AnyAsync(x => x.Id == peliculaId);
            //if (!existePelicula) { return NotFound(); }

            var queryable = context.Reviews.Include(x => x.Usuario).AsQueryable();
            queryable = queryable.Where(x => x.PeliculaId == peliculaId);
            return await Get<Review, ReviewDTO>(paginacionDTO, queryable);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //Solo usuarios autenticados van a poder escribir comentarios
        public async Task<ActionResult> Post(int peliculaId, [FromBody] ReviewCreacionDTO reviewCreacionDTO)
        {
            //Encapsulando lógica que se repite en el helper PeliculaExisteAttribute
            //var existePelicula = await context.Peliculas.AnyAsync(x => x.Id == peliculaId);
            //if (!existePelicula) { return NotFound(); }

            /*
             * En la clase ReviewCreacionDTO no se colocó como variable el id del usuario porque es muy mala práctica exponer su id.
             * La información de quién es el usuario debe venir el jwt (del claim NameIdentifier)
             */
            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var reviewExiste = await context.Reviews.AnyAsync(x => x.PeliculaId == peliculaId && x.UsuarioId == usuarioId);
            if (reviewExiste) { return BadRequest("El usuario ya ha escrito un review de esta película"); }

            var review = mapper.Map<Review>(reviewCreacionDTO);
            review.PeliculaId = peliculaId;
            review.UsuarioId = usuarioId;

            context.Add(review);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int peliculaId ,int reviewId, [FromBody] ReviewCreacionDTO reviewCreacionDTO)
        {
            //Encapsulando lógica que se repite en el helper PeliculaExisteAttribute
            //var existePelicula = await context.Peliculas.AnyAsync(x => x.Id == peliculaId);
            //if (!existePelicula) { return NotFound(); }

            var reviewDB = await context.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);
            if (reviewDB == null) { return NotFound(); }

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if(reviewDB.UsuarioId!=usuarioId) { return BadRequest("No tiene permisos para editar este review"); }

            //if (reviewDB.UsuarioId != usuarioId) { return Forbid(); } //Si el usuario que escribió el review no es el usuario que quiere modificarlo, entocesle vamos a prohibir

            reviewDB = mapper.Map(reviewCreacionDTO, reviewDB); //Estamos tomando los cambios almacenado en reviewCreacionDTO y se lo estamos pasando a reviewDB y entityFramework nos ayuda a actulizar los cambios o las diferencias que hay entre ambas clases

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int reviewId)
        {
            //Encapsulando lógica que se repite en el helper PeliculaExisteAttribute
            //var existePelicula = await context.Peliculas.AnyAsync(x => x.Id == peliculaId);
            //if (!existePelicula) { return NotFound(); }

            var reviewDB = await context.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);
            if (reviewDB == null) { return NotFound(); }

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if(usuarioId != reviewDB.UsuarioId) { return Forbid(); }

            context.Remove(reviewDB);
            await context.SaveChangesAsync();
            
            return NoContent();
        }
        
    }
}
