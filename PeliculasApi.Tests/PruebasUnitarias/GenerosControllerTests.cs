using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeliculasApi.Controllers;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class GenerosControllerTests : BasePruebas
    {
        [TestMethod]
        public async Task ObtenerTodosLosGeneros()
        {
            //Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            contexto.Add(new Genero() { Nombre = "Género 1" });
            contexto.Add(new Genero() { Nombre = "Género 2" });
            await contexto.SaveChangesAsync();

            /*
             * Se debe crear otro contexto, ya que el primer contexto se encuentra en memoria,
             * por lo que instanciamos contexto2 para asegurarnos que realmente estamos yendo a la bbdd y no lo leemos de la memoria
            */
            var contexto2 = ConstruirContext(nombreBD);

            //Prueba
            var controller = new GenerosController(contexto2, mapper);
            var respuesta = await controller.Get();

            //Verificación
            var generos = respuesta.Value;
            Assert.AreEqual(2, generos.Count);
        }

        [TestMethod]
        public async Task ObtenerGeneroPorIdInexistente()
        {
            //Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            //Prueba
            //No vamos a crear ningún, ya que vamos a buscar un género que no existe
            var controller = new GenerosController(contexto, mapper);
            var respuesta = await controller.Get(1); //Le podemos cualquier atributo, xq no existe ningún género

            //Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task ObtenerGeneroPorIdExistente()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            await contexto.Generos.AddAsync(new Genero() { Nombre = "Género 1" });
            await contexto.Generos.AddAsync(new Genero() { Nombre = "Género 2" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);
            var controller = new GenerosController(contexto2, mapper);
            
            int id = 1;
            var respuesta = await controller.Get(id);
            var resultado = respuesta.Value;//Resultado es de tipo GeneroDTO

            Assert.AreEqual(id, resultado.Id);
        }

        [TestMethod]
        public async Task CrearGenero()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            var nuevoGenero = new GeneroCreacionDTO() { Nombre = "Nuevo Género" };

            var controller = new GenerosController(contexto, mapper);
            var respuesta = await controller.Post(nuevoGenero);
            var resultado = respuesta as CreatedAtRouteResult; //Darse cuenta que el ActionResult Post retorna un CreatedAtRouteResult, por eso lo convertimos
            Assert.IsNotNull(resultado);//primera validación

            //También validamos que la bbdd retorna el número de generos creados
            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Generos.CountAsync();
            Assert.AreEqual(1, cantidad);
        }

        [TestMethod]
        public async Task ActualizarGenero()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            contexto.Add(new Genero() { Nombre = "Género 1" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);
            var controller = new GenerosController(contexto2, mapper);

            var generoCreacionDTO = new GeneroCreacionDTO() { Nombre = "Nuevo Nombre" };

            var id = 1;
            var respuesta = await controller.Put(id, generoCreacionDTO);

            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado.StatusCode);

            //Verificamos si el género modificado está en la bbdd
            var contexto3 = ConstruirContext(nombreBD);
            var existe = await contexto3.Generos.AnyAsync(x => x.Nombre == "Nuevo Nombre");
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task BorrarGeneroNoExistente()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            var controller = new GenerosController(contexto, mapper);
            var id = 1;
            var respuesta = await controller.Delete(id);
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task BorrarGeneroExistente()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            contexto.Generos.Add(new Genero() { Nombre = "Género 1" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);
            var controller = new GenerosController(contexto2, mapper);

            var id = 1;
            var respuesta = await controller.Delete(id);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado.StatusCode);

            //Vamos a ir a la bbdd y asegurar que no hay géneros en la tabla
            var contexto3 = ConstruirContext(nombreBD);
            var existe = await contexto.Generos.AnyAsync();
            Assert.IsFalse(existe);

        }
    }
}
