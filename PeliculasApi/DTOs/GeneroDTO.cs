using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class GeneroDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Campo requerido")]
        [MaxLength(40, ErrorMessage = "Ha sobrepasado los 40 carácteres")]
        public string Nombre { get; set; }
    }
}
