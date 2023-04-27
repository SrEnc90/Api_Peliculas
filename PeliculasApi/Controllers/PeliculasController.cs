using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using PeliculasApi.Migrations;
using PeliculasApi.Servicios;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;

namespace PeliculasApi.Controllers
{
    [Route("api/peliculas")]
    [ApiController]
    public class PeliculasController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<PeliculasController> logger;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context,
            IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos,
            ILogger<PeliculasController> logger)
            : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
        }

        [HttpGet()]
        public async Task<ActionResult<PeliculasIndexDTO>> Get()
        {
            //lógica para filtros por defecto en el listado de películas:
            var top = 5;
            var hoy = DateTime.Today;

            var proximosEstremos = await context.Peliculas
                .Where(x => x.FechaEstreno < hoy)
                .OrderBy(x => x.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(x => x.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDTO();
            resultado.FuturosEstrenos = mapper.Map<List<PeliculaDTO>>(proximosEstremos);
            resultado.EnCines = mapper.Map<List<PeliculaDTO>>(enCines);

            return resultado;
        }

        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] FiltroPeliculasDTO filtroPeliculasDTO)//Necesitamos el [FromQuery] para utilizar un tipo complejo como parámetro de entrada en los métodos de tipo get
        {
            var peliculasQueryable = context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.Titulo))
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.Titulo.Contains(filtroPeliculasDTO.Titulo));
            }

            if (filtroPeliculasDTO.EnCines)
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.EnCines);
            }

            if (filtroPeliculasDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable.Where(x => x.FechaEstreno > hoy);
            }

            if (filtroPeliculasDTO.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.PeliculasGeneros.Select(y => y.GeneroId)
                    .Contains(filtroPeliculasDTO.GeneroId));
            }

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.CampoOrdenar))
            {
                var tipoOrden = filtroPeliculasDTO.OrderAscendente ? "ascending" : "descending";
                try
                {
                    peliculasQueryable = peliculasQueryable.OrderBy($"{filtroPeliculasDTO.CampoOrdenar} {tipoOrden}"); //Estoy usando el OrderBy de la librería Linq.Dynamic.Core
                }
                catch (Exception ex)
                {
                    //Minimamente se debe guardar el error en el logger
                    logger.LogError(ex.Message, ex);
                }
            }

            await HttpContext.InsertarParametrosPaginacion(peliculasQueryable,
                filtroPeliculasDTO.CantidadRegistrosPorPagina);

            var peliculas = await peliculasQueryable.Paginar(filtroPeliculasDTO.Paginacion).ToListAsync();

            return mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpGet("{id:int}", Name = "obtenerPelicula")]
        //Cambiamos el PeliculaDTO por PeliculaDetallesDTO(el cual hereda de PeliculaDTO) para poder relacionar la data con la de generos y actores y poder mostrarla
        public async Task<ActionResult<PeliculaDetallesDTO>> Get(int id) 
        {
            var pelicula = await context.Peliculas
                // Al usar .Include navegamos por la variable PeliculasGeneros del la clase película y con .ThenInclude navegamos a la variable Genero dentro de la clase PeliculasActores
                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero) 
                .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if(pelicula == null) { return NotFound(); }

            pelicula.PeliculasActores = pelicula.PeliculasActores.OrderBy(x => x.Orden).ToList();

            return mapper.Map<PeliculaDetallesDTO>(pelicula);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);

            if (peliculaCreacionDTO.Poster != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await peliculaCreacionDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculaCreacionDTO.Poster.FileName);
                    pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, 
                                contenedor, peliculaCreacionDTO.Poster.ContentType);
                }
            }
            AsignarOrdenActores(pelicula);
            context.Add(pelicula);
            await context.SaveChangesAsync();
            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);
            return new CreatedAtRouteResult("obtenerPelicula", new { id = pelicula.Id }, peliculaDTO);
        }

        private void AsignarOrdenActores(Pelicula pelicula)
        {
            if (pelicula.PeliculasActores != null)
            {
                for(int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var peliculaDB = await context.Peliculas
                .Include(x => x.PeliculasGeneros)
                .Include(x => x.PeliculasActores) //Para poder instanciar el método AsignarOrdenActores(pelicula)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (peliculaDB == null) { return NotFound(); }

            peliculaDB = mapper.Map(peliculaCreacionDTO, peliculaDB);//entity framework nos ayuda a mapear los campos que se han modificado con respecto del recurso de destino y los modifica

            if (peliculaCreacionDTO.Poster != null)
            {
                using(MemoryStream memoryStream=new MemoryStream())
                {
                    await peliculaCreacionDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculaCreacionDTO.Poster.FileName);

                    peliculaDB.Poster = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor, peliculaDB.Poster, 
                                        peliculaCreacionDTO.Poster.ContentType);
                }
            }
            AsignarOrdenActores(peliculaDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PeliculaPatchDTO> patchDocument)
        {
            return await Patch<Pelicula, PeliculaPatchDTO>(id, patchDocument);
            /*
            if(patchDocument == null) { return BadRequest(); }//Error del usuario, por eso es BadRequest
            
            var entidadDB = await context.Peliculas.FirstOrDefaultAsync(p => p.Id == id);
            if (entidadDB == null) { return NotFound(); }
            
            var entidadDTO = mapper.Map<PeliculaPatchDTO>(entidadDB);
            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);

            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(entidadDTO, entidadDB);

            await context.SaveChangesAsync();

            return NoContent();
            */
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Pelicula>(id);
            /*
            var existe = await context.Peliculas.AnyAsync(p => p.Id == id);
            if (!existe) { NotFound(); }

            context.Remove(new Pelicula { Id = id });
            await context.SaveChangesAsync();

            return NoContent();
            */
        }
    }
}
