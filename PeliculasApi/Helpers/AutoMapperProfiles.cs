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

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore()); //para evitar problemas con foto que son de diferente tipo de dato
            CreateMap<ActorPatchDTO, Actor>().ReverseMap();

            CreateMap<Pelicula, PeliculaDTO>().ReverseMap();
            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(x => x.Poster, options => options.Ignore());
            CreateMap<PeliculaPatchDTO, Pelicula>().ReverseMap();
        }
    }
}
