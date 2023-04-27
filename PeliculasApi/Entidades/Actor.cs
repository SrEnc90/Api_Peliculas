using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Entidades
{
    public class Actor : IId //implementamos la interfaz para que implemente el campo Id
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength:120, ErrorMessage = "Debe ser menor a 120 carácteres")]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Foto { get; set; }//Vamos a guardar la URL de la imagen
        public List<PeliculasActores> PeliculasActores { get; set; }
    }
}
