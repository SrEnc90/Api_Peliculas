using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using PeliculasApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi.Tests
{
    //Se crea una clase BasePruebas para que cada uno de los controllers que vamos a probar utilicen el DbContext y el mapper de esta clase
    public class BasePruebas
    {
        protected string usuarioPorDefectoId = "9722b56z-77ea-4e41-941d-e319b6eb3712";
        protected string usuarioPorDefectoEmail = "ejemplo@hotmail.com";

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

        //Método que nos va a ayudar insertar claims en el httpContext para testear el ReviewController
        protected ControllerContext ConstruirControllerContext()
        {
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name,usuarioPorDefectoEmail),
                new Claim(ClaimTypes.Email,usuarioPorDefectoEmail),
                new Claim(ClaimTypes.NameIdentifier,usuarioPorDefectoId)
            }));

            return new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = usuario }
            };
        }

        //Para utilizar el WebApplicationFactory debemos instalar el paquete Microsoft.AspNetCore.MVC.Testing y cambiar el csprofile del proyecto
        protected WebApplicationFactory<Startup> ConstruirWebApplicationFactory(string nombreBD,
            bool ignorarSeguridad = true) // ignorarSeguridad es para ignorar el authorize de los endpoints que lo utilicen (saltarnos la medida de seguiridad para realizar las pruebas)
        {
            var factory = new WebApplicationFactory<Startup>();
            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    //Desdes aquí podemos configurar el sistema de inyección de dependencias de la aplicación
                    //Utilizar nuestro proveedor en memoria de EntityFrameworkCore para lo cuál tenemos que remover el servicio de DbContext del startUp
                    var descriptorBDContext = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptorBDContext != null)
                    {
                        services.Remove(descriptorBDContext);
                    }
                    //Después de eliminarlo agregamos un nuevo DBContext pero en esta utilizamos el proveedor en memoria
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(nombreBD));

                    if (ignorarSeguridad)
                    {
                        //Saltarnos la medidas de seguridad de atributos como authorize con la clase AllowAnonymousHandler que hemos creado
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new UsuarioFalsoFiltro()); //Gracias a esto tenemos acceso a claims que necesitan información del usuario. Ejm: controllador de Reviews
                        });
                    }
                });
            });
            return factory;
        }
    }
}
