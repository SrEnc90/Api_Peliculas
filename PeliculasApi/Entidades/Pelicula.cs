using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Entidades
{
    public class Pelicula : IId //implementamos la interfaz para que implemente el campo Id
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(300, ErrorMessage = "El título no debe tener más de 300 carácteres")]
        public string Titulo { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaEstreno { get; set; }
        public string Poster { get; set; }
        public List<PeliculasActores> PeliculasActores { get; set; }
        public List<PeliculasGeneros> PeliculasGeneros { get; set; }
        public List<PeliculasSalasDeCine> PeliculasSalasDeCines { get; set; }
    }
}
