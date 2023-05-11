using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using PeliculasApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi.Tests
{
    //Se crea una clase BasePruebas para que cada uno de los controllers que vamos a probar utilicen el DbContext y el mapper de esta clase
    public class BasePruebas
    {
        //Cada prueba unitaria va a crear su propia base de datos(cada prueba debe ser independiente de la otra)
        //Nosotros le vamos a pasar el nombre de la bbdd que vamos a probar
        protected ApplicationDbContext ConstruirContext(string nombreDB)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDB).Options; // Para usar UseInMemoryDatabase hemos instalado Microsoft.EntityFrameworkCore.InMemory, el cuál permite cargar una base de datos en memoria

            var dbContext = new ApplicationDbContext(opciones);

            return dbContext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                //La clase Helpers.AutoMapperProfiles dentro del proyecto PeliculasApi tiene una depedencia:
                //En su controlador está instanciada  la librería GeometryFactory, por lo que aquí también se le debe pasar
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfiles(geometryFactory));
            });

            return config.CreateMapper();
        }
    }
}
