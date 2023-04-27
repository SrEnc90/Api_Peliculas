using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class SalaDeCineCreacionDTO
    {
        [Required]
        [StringLength(120, ErrorMessage = "No puede exceder los 120 carácteres")]
        public string Nombre { get; set; }
    }
}
