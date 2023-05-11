namespace PeliculasApi.DTOs
{
    public class ReviewDTO //dto de lectura para leer los registros almacenados en la base de datos
    {
        public int Id { get; set; }
        public string Comentario { get; set; }
        public int Puntuacion { get; set; }
        public int PeliculaId { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
    }
}
