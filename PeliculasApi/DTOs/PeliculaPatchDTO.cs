using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class PeliculaPatchDTO
    {
        [Required]
        [StringLength(300, ErrorMessage = "El título no debe tener más de 300 carácteres")]
        public string Titulo { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaEstrenos { get; set; }
    }
}
