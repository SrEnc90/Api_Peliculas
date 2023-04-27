namespace PeliculasApi.DTOs
{
    public class FiltroPeliculasDTO
    {
        //Paginación del filtro
        public int Pagina { get; set; } = 1;
        public int CantidadRegistrosPorPagina { get; set; } = 10;
        public PaginacionDTO Paginacion 
        {
            get { return new PaginacionDTO() { Pagina = Pagina, CantidadRegistrosPorPagina = CantidadRegistrosPorPagina }; } 
        }
        //Los filtros que vamos a realizar
        public string Titulo { get; set; }
        public int GeneroId { get; set; }
        public bool EnCines { get; set; }
        public bool ProximosEstrenos { get; set; }

        //Filtros para ordernar ascendente o descendente en el action result Filtrar  
        public string CampoOrdenar { get; set; }
        public bool OrderAscendente { get; set; } = true;
    }
}
