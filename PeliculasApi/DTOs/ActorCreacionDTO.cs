using PeliculasApi.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class ActorCreacionDTO : ActorPatchDTO
    {
        //[PesoArchivoValidacion(4)] Otra forma de colocarlo
        [PesoArchivoValidacion(pesoMaximoEnMegaByte: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile Foto { get; set; }//No vamos a recibir la ruta, sino la foto como tal(un archivo)
    }
}
