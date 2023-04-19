namespace PeliculasApi.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;

        private int _cantidadRegistrosPorPagina = 10;

        private readonly int _cantidadMaximaRegistrosPorPagina = 50;

        public int CantidadRegistrosPorPagina
        {
            get => _cantidadRegistrosPorPagina;

            set
            {
                _cantidadRegistrosPorPagina = value > _cantidadMaximaRegistrosPorPagina ? _cantidadMaximaRegistrosPorPagina : value;
            }
        }
    }
}
