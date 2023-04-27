using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Entidades
{
    public class SalaDeCine : IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(120, ErrorMessage = "No debe exceder los 120 caracteres")]
        public string Nombre { get; set; }
        public List<PeliculasSalasDeCine> PeliculasSalasDeCines { get; set; }
    }
}
