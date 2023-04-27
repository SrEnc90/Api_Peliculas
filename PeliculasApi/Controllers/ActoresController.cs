using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using PeliculasApi.Migrations;
using PeliculasApi.Servicios;

namespace PeliculasApi.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor = "actores";

        public ActoresController(ApplicationDbContext context, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos)
            : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }


        /*
         *Para poder colocar la paginación en la cabecera: vamos a utilizar querystring: por lo que es necesario utilizar [FromQuery]
         */
        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            return await Get<Actor, ActorDTO>(paginacionDTO);
            /*
            var querable = context.Actores.AsQueryable();

            //Amobs métodos son extensiones de sus respectivas clases y se encuentran creadas en la carpeta de Helpers
            await HttpContext.InsertarParametrosPaginacion(querable, paginacionDTO.CantidadRegistrosPorPagina);
            var entidades = await querable.Paginar(paginacionDTO).ToListAsync();

            return mapper.Map<List<ActorDTO>>(entidades);
            */
        }

        [HttpGet("{id}", Name = "obtenerActor")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            return await Get<Actor, ActorDTO>(id);
            /*
            var entidad = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null) { return NotFound(); }

            return mapper.Map<ActorDTO>(entidad);
            */
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreacionDTO actorCreacionDTO) //Vamos a recibir la foto desde el formulario
        {
            var entidad = mapper.Map<Actor>(actorCreacionDTO);

            if (actorCreacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
                    //Recuerda que en actorCreacionDTO se trabaja con el archivo en si, pero en la entidad Actor se guarda la ruta dónde está ubicado la foto
                    entidad.Foto = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor,
                        actorCreacionDTO.Foto.ContentType);
                }
            }

            context.Add(entidad);

            await context.SaveChangesAsync();

            var dto = mapper.Map<ActorDTO>(entidad);

            return new CreatedAtRouteResult("obtenerActor", new { id = entidad.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actorDB = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (actorDB == null) { return NotFound(); }
            /*
             * Lo que estamos haciendo acá, net core nos ayuda actualizando solo los campos que no concuerndan 
             * entre ambas instancias en el mapeo, esto debido a q no siempre se va a mandar todos los campos a actualizar, 
             * puede que solo 1 o más
             */
            actorDB = mapper.Map(actorCreacionDTO, actorDB);

            if (actorCreacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();//Lo convertido a un arreglo de bytes para mandarlo al AlmacenadorArchivos
                    var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
                    /*
                     * Recuerda que el EditarArchivo y el GuardarArchivo retornan la ruta dónde se va a almacenar y
                     * que el ActorDB.Foto es un string dónde se almacena la ruta de la foto y 
                     * actorCreacionDTO.Foto es un IFormFile, dónde se almancena el archivo en si
                     */
                    actorDB.Foto = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor,
                        actorDB.Foto, actorCreacionDTO.Foto.ContentType);
                }
            }

            await context.SaveChangesAsync();

            return NoContent();
        }

        /*
         * Para poder utilizar el httpPatch, debemos instalar el paquete Microsoft.AspNetCore.JsonPatchDocument
         * Para poder utilizar el ModelState debemos instalar el paquete de Microsoft.AspNetCore.Mvc.NewtonsoftJson (no es usuario usar el using namespace)
         * Debemos configurar servicio el NewtonsoftJson en el startup
         */

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDTO> patchDocument)
        {
            return await Patch<Actor, ActorPatchDTO>(id, patchDocument);
            /*
            if (patchDocument == null) { return BadRequest(); }

            var entidadDB = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (entidadDB == null) { return NotFound(); }

            var entidadDTO=mapper.Map<ActorPatchDTO>(entidadDB);

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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Actor>(id);
            /*
            var existe = await context.Actores.AnyAsync(x => x.Id == id);

            if(!existe) { return NotFound(); }

            context.Remove(new Actor { Id = id });

            await context.SaveChangesAsync();

            return NoContent();
            */
        }
    }
}
