using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class ActorPatchDTO
    {
        [Required]
        [StringLength(maximumLength: 120, ErrorMessage = "Debe ser menor a 120 carácteres")]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
