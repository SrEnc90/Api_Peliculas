using AutoMapper;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Migrations;

namespace PeliculasApi.Helpers
{
    //Para que esta clase sea un perfil de automapper, debemos heredar de Profile
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap(); //Convertirmos objetos a generoDTO y con ReserverMap indicamos que tb lo vamos hacer al revés
            CreateMap<GeneroCreacionDTO, Genero>(); //Del GeneroCreacionDTo lo voy a pasar a Genero y de ahí a la BBDD

            CreateMap<SalaDeCine,SalaDeCineDTO>().ReverseMap();
            CreateMap<SalaDeCineCreacionDTO,SalaDeCine>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore()); //para evitar problemas con foto que son de diferente tipo de dato
            CreateMap<ActorPatchDTO, Actor>().ReverseMap();

            CreateMap<Pelicula, PeliculaDTO>().ReverseMap();
            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(x => x.Poster, options => options.Ignore())
                //Mapeando la relación de muchos a muchos
                .ForMember(x => x.PeliculasGeneros, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapPeliculasActores));

            //Para relacionar la data de películas con actores y géneros en el método get
            CreateMap<Pelicula, PeliculaDetallesDTO>()
                .ForMember(x => x.Generos, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.Actores, options => options.MapFrom(MapPeliculasActores));

            CreateMap<PeliculaPatchDTO, Pelicula>().ReverseMap();
        }

        private List<ActorPeliculaDetalleDTO> MapPeliculasActores(Pelicula pelicula, PeliculaDetallesDTO peliculaDetallesDTO)
        {
            var resultado = new List<ActorPeliculaDetalleDTO>();
            if (pelicula.PeliculasActores == null) { return resultado; }
            foreach (var actorPelicula in pelicula.PeliculasActores)
            {
                resultado.Add(new ActorPeliculaDetalleDTO()
                {
                    ActorId = actorPelicula.ActorId,
                    Personaje = actorPelicula.Personaje,
                    NombrePersona = actorPelicula.Actor.Nombre
                });
            }
            return resultado;
        }

        private List<GeneroDTO> MapPeliculasGeneros(Pelicula pelicula, PeliculaDetallesDTO peliculaDetallesDTO)
        {
            var resultado = new List<GeneroDTO>();
            if (pelicula.PeliculasGeneros == null) { return resultado; }
            foreach (var generoPelicula in pelicula.PeliculasGeneros)
            {
                resultado.Add(new GeneroDTO() { Id = generoPelicula.GeneroId, Nombre = generoPelicula.Genero.Nombre });
            }
            return resultado;
        }

        private List<PeliculasGeneros> MapPeliculasGeneros(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasGeneros>();
            if (peliculaCreacionDTO.GeneroIDs == null) { return resultado; }
            foreach(var id in peliculaCreacionDTO.GeneroIDs)
            {
                resultado.Add(new PeliculasGeneros() { GeneroId = id });
            }
            return resultado;
        }

        private List<PeliculasActores> MapPeliculasActores(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var restulado = new List<PeliculasActores>();
            if (peliculaCreacionDTO.Actores == null) { return restulado; }
            foreach (var actor in peliculaCreacionDTO.Actores)
            {
                restulado.Add(new PeliculasActores() { ActorId = actor.ActorId, Personaje = actor.Personaje });
            }
            return restulado; 
        }
    }
}
