using AutoMapper;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Migrations;

namespace PeliculasApi.Helpers
{
    //Para que esta clase sea un perfil de automapper, debemos heredar de Profile
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap(); //Convertirmos objetos a generoDTO y con ReserverMap indicamos que tb lo vamos hacer al revés
            CreateMap<GeneroCreacionDTO, Genero>(); //Del GeneroCreacionDTo lo voy a pasar a Genero y de ahí a la BBDD

            CreateMap<Review, ReviewDTO>()
                .ForMember(x => x.NombreUsuario, x => x.MapFrom(y => y.Usuario.UserName)); //En la clase ReviewDTO tenemos el NombreUsuario que provieve de la clase Usuario, debemos navegar hasta la clase Usuario
            CreateMap<ReviewDTO, Review>();
            CreateMap<ReviewCreacionDTO, Review>();

            CreateMap<IdentityUser, UsuarioDTO>();

            //CreateMap<SalaDeCine,SalaDeCineDTO>().ReverseMap(); la converción va hacer complejo, por lo que mejor se elimina el ReverseMap y se coloca individualmente en dos líneas
            CreateMap<SalaDeCine, SalaDeCineDTO>()
                .ForMember(x => x.Latitud, x => x.MapFrom(y => y.Ubicacion.Y)) // mapeando desde el Point hacia la latitud
                .ForMember(x => x.Longitud, x => x.MapFrom(y => y.Ubicacion.X)); // mapeando desde el Point hacia la longitud

            CreateMap<SalaDeCineDTO, SalaDeCine>() //convirtiendo desde la latitud y longitud hacia un point, para ello pasamos como parámetro del constructor el GeometryFactory
                .ForMember(x => x.Ubicacion, x => x.MapFrom(y =>
                    geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

            CreateMap<SalaDeCineCreacionDTO,SalaDeCine>()
                 .ForMember(x => x.Ubicacion, x => x.MapFrom(y =>
                    geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

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
