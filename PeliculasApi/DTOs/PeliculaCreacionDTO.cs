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
        [ModelBinder(BinderType = typeof(TypeBinder))]
        public List<int> GeneroIDs { get; set; }
    }
}
