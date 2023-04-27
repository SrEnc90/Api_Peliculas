using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Entidades
{
    public class Genero : IId //implementamos la interfaz para que implemente el campo Id
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo requerido")]
        [MaxLength(40,ErrorMessage = "Ha sobrepasado los 40 carácteres")]
        public string Nombre { get; set; }
        public List<PeliculasGeneros> PeliculasGeneros { get; set; }
    }
}
