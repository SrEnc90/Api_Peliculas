using Microsoft.AspNetCore.Mvc;
using PeliculasApi.Helpers;
using PeliculasApi.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class PeliculaCreacionDTO : PeliculaPatchDTO
    {
        [PesoArchivoValidacion(pesoMaximoEnMegaByte: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }

        //Queremos recibir el listado Id de generos para crear la película con los genéros al cuál pertenece
        /*
         * Al utilizar FromForm (ver los parámetros enviados al controlador POST de creación de película)
         * modifica el comportamiento por defecto del ModelBinder, por lo que creamos un helper, para indicarle lo que debe hacer
         */
        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GeneroIDs { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<ActorPeliculasCreacionDTO>>))]
        public List<ActorPeliculasCreacionDTO> Actores { get; set; }
    }
}
