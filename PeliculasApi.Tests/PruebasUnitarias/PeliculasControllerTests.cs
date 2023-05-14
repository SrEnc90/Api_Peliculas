using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeliculasApi.Entidades;
using PeliculasApi.Controllers;
using PeliculasApi.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class PeliculasControllerTests : BasePruebas
    {
        private string CrearDataPrueba()
        {
            var databaseName = Guid.NewGuid().ToString();
            var context = ConstruirContext(databaseName);
            var genero = new Genero() { Nombre = "genre 1" };

            var peliculas = new List<Pelicula>()
            {
                new Pelicula() { Titulo = "Película 1", FechaEstreno = new DateTime(2010,01,01), EnCines = false },
                new Pelicula() { Titulo = "No Estrenada", FechaEstreno = DateTime.Today.AddDays(1), EnCines = false },
                new Pelicula() { Titulo = "Película en Cines", FechaEstreno = DateTime.Today.AddDays(-1), EnCines = true }
            };

            var peliculaConGenero = new Pelicula()
            {
                Titulo = "Película con Género",
                FechaEstreno = new DateTime(2010, 01, 01),
                EnCines = false
            };
            peliculas.Add(peliculaConGenero);

            context.Add(genero);
            context.AddRange(peliculas);
            context.SaveChanges();

            var peliculaGenero = new PeliculasGeneros() { GeneroId = genero.Id, PeliculaId = peliculaConGenero.Id };
            context.Add(peliculaGenero);
            context.SaveChanges();

            return databaseName;
        }

        [TestMethod]
        public async Task FiltrarPorTitulo()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var tituloPelicula = "Película 1";
            var filtroDTO = new FiltroPeliculasDTO()
            {
                Titulo = tituloPelicula,
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual(tituloPelicula, peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarEnCine()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculasDTO()
            {
                EnCines = true
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("Película en Cines", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarProximosEstrenos()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculasDTO()
            {
                ProximosEstrenos = true
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("No Estrenada", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarPorGenero()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var generoId = await contexto.Generos.Select(x => x.Id).FirstOrDefaultAsync();
            var filtroDTO = new FiltroPeliculasDTO()
            {
                GeneroId = generoId
            };

            var resultado = await controller.Filtrar(filtroDTO);
            var peliculas = resultado.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("Película con Género", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarOrdenTituloAscendente()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculasDTO()
            {
                CampoOrdenar = "titulo",
                OrderAscendente = true
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            //Para comprobar que está ordenado, debemos contruir un nuevo contexto con el mismo nombre de base de datos el cual ordenamos y comparamos ambos objetos
            var contexto2 = ConstruirContext(nombreBD);
            var peliculasDB = await contexto2.Peliculas.OrderBy(x => x.Titulo).ToListAsync();

            Assert.AreEqual(peliculas.Count, peliculasDB.Count);

            for (int i = 0; i < peliculasDB.Count; i++)
            {
                var peliculaDelControlador = peliculasDB[i];
                var peliculaDB = peliculasDB[i];

                Assert.AreEqual(peliculaDB.Id, peliculaDelControlador.Id);
            }
        }

        [TestMethod]
        public async Task FiltrarOrdenTituloDesscendente()
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculasDTO
            {
                CampoOrdenar = "titulo",
                OrderAscendente = false
            };

            var resultado = await controller.Filtrar(filtroDTO);
            var peliculas = resultado.Value;

            var contexto2 = ConstruirContext(nombreBD);
            var peliculasBD = await contexto2.Peliculas.OrderByDescending(x => x.Titulo).ToListAsync();

            Assert.AreEqual(peliculasBD.Count(), peliculas.Count());

            for (int i = 0; i < peliculasBD.Count; i++)
            {
                var peliculasDelControlador = peliculas[i];
                var peliculaBD = peliculasBD[i];

                Assert.AreEqual(peliculaBD.Id, peliculasDelControlador.Id);
            }
        }

        [TestMethod]
        public async Task FiltrarPorCampoIncorrectoDevuelvePeliculas() //En el método Filtrar del controlador le indicamos que en caso de error devuelva todos losr registros
        {
            var nombreBD = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContext(nombreBD);

            //También queremos comprobar que cuando si hay error, efectivamente se está logeando el error
            var mock = new Mock<ILogger<PeliculasController>>(); //Verificar que proviene de Microsoft.Extensions.Logging
            var controller = new PeliculasController(contexto, mapper, null, mock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculasDTO()
            {
                CampoOrdenar = "abc",
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContext(nombreBD);
            var peliculasBD = await contexto2.Peliculas.ToListAsync();

            Assert.AreEqual(peliculasBD.Count, peliculas.Count);
            Assert.AreEqual(1, mock.Invocations.Count);
        }
    }
}
