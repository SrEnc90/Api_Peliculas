using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Entidades
{
    public class Pelicula
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(300, ErrorMessage = "El título no debe tener más de 300 carácteres")]
        public string Titulo { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaEstreno { get; set; }
        public string Poster { get; set; }
    }
}
