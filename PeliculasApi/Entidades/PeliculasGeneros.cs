namespace PeliculasApi.Entidades
{
    //Estableciendo la relación de muchos a muchos entre películas y genéros
    public class PeliculasGeneros
    {
        public int GeneroId { get; set; }
        public int PeliculaId { get; set; }
        public Genero Genero { get; set; }
        public Pelicula Pelicula { get; set; }
    }
}
