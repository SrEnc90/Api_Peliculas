using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System.Net.Mime;

namespace PeliculasApi.Controllers
{
    [ApiController]
    [Route("api/generos")]
    public class GenerosController : CustomBaseController
    {
        public GenerosController(ApplicationDbContext context, 
            IMapper mapper)
            :base(context,mapper) //el base representa el constructor de la clase que heredamo y q en esta situación le pasamos el context y el mapper
        {
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get()
        {
            return await Get<Genero, GeneroDTO>();//Estoy utilizando el CustomBaseController
        }

        [HttpGet("{id:int}", Name = "obtenerGenero")]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            return await Get<Genero, GeneroDTO>(id);//Estoy utilizando el CustomBaseController
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            return await Post<GeneroCreacionDTO, Genero, GeneroDTO>(generoCreacionDTO, "obtenerGenero");
            /*
             * Estamos utilizando el método que la clase CustomBaseController
            var entidad = mapper.Map<Genero>(generoCreacionDTO);

            context.Add(entidad);

            await context.SaveChangesAsync();

            //Estoy mapeando el objeto Genero a GeneroDTO
            var generoDTO = mapper.Map<GeneroDTO>(entidad);

            //Debemos colocarle nombre a la ruta o endpoint que vamos a redireccionar
            return new CreatedAtRouteResult("obtenerGenero", new { id = generoDTO.Id }, generoDTO);
            */
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            return await Put<GeneroCreacionDTO, Genero>(id, generoCreacionDTO);

            /*
            var entidad = mapper.Map<Genero>(generoCreacionDTO);

            entidad.Id = id;

            context.Entry(entidad).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return NoContent();
            */
        }

        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Genero>(id);

            /*
            var existe = await context.Generos.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Genero() { Id = id }); //Para identificar el género solo basta con pasar su id

            await context.SaveChangesAsync();

            return NoContent();
            */
        }
    }
}
