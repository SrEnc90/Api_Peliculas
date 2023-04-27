using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using System.Xml.XPath;

namespace PeliculasApi.Controllers
{
    public class CustomBaseController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CustomBaseController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        //Protected es un modificador, que restringe el acceso a la clase solo a la propia clase o a sus subclases(Herencia).Se puede utilizar en la clases o en su métodos
         protected async Task<List<TDTO>> Get<TEntidad, TDTO>() where TEntidad : class
        {
            var entidades = await context.Set<TEntidad>().AsNoTracking().ToListAsync(); //Con el AsNoTracking vamos hacer un poco más rápida la consulta a la db
            var dtos = mapper.Map<List<TDTO>>(entidades);
            return dtos;
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDTO paginacionDTO) 
            where TEntidad: class
        {
            var querable = context.Set<TEntidad>().AsQueryable();

            //Ambos métodos son extensiones de sus respectivas clases y se encuentran creadas en la carpeta de Helpers
            await HttpContext.InsertarParametrosPaginacion(querable, paginacionDTO.CantidadRegistrosPorPagina);
            var entidades = await querable.Paginar(paginacionDTO).ToListAsync();

            return mapper.Map<List<TDTO>>(entidades);
        }

        protected async Task<ActionResult<TDTO>> Get<TEntidad, TDTO>(int id) where TEntidad: class, IId //utilizamos la interfaz creada(IId) para asegurarnos que debe tener campo de Id
        {
            var entidad = await context.Set<TEntidad>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if(entidad == null) { NotFound(); }
            return mapper.Map<TDTO>(entidad);
        }

        protected async Task<ActionResult> Post<TCreacion, TEntidad, TLectura>
            (TCreacion creacionDTO, string nombreRuta) where TEntidad : class, IId //dónde TEntidad es una clase que hereda de la interfaz IId(eso significa)
        {
            var entidad = mapper.Map<TEntidad>(creacionDTO);

            context.Add(entidad);

            await context.SaveChangesAsync();

            //Estoy mapeando el objeto Genero a GeneroDTO
            var dtoLectura = mapper.Map<TLectura>(entidad);

            //Debemos colocarle nombre a la ruta o endpoint que vamos a redireccionar
            return new CreatedAtRouteResult("obtenerGenero", new { id = entidad.Id }, dtoLectura);
        } 

        protected async Task<ActionResult> Put<TCreacion, TEntidad>
            (int id, TCreacion creacionDTO) where TEntidad : class, IId
        {
            var entidad = mapper.Map<TEntidad>(creacionDTO);

            entidad.Id = id;

            context.Entry(entidad).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Patch<TEntidad,TDTO>(int id, JsonPatchDocument<TDTO> patchDocument)
            where TDTO : class
            where TEntidad : class, IId
        {
            if (patchDocument == null) { return BadRequest(); }

            var entidadDB = await context.Set<TEntidad>().FirstOrDefaultAsync(x => x.Id == id);

            if (entidadDB == null) { return NotFound(); }

            var entidadDTO = mapper.Map<TDTO>(entidadDB);

            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);
            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(entidadDTO, entidadDB);

            await context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Delete<TEntidad>
            (int id) where TEntidad : class, IId, new() // el new () indica que tiene un constructor vacío y es importante para poder instanciar el constuctor vacío en el context.Remove(new TEntidad()
        {
            var existe = await context.Set<TEntidad>().AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new TEntidad() { Id = id }); //Para identificar el género solo basta con pasar su id

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
