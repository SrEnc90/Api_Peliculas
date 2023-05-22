using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;

namespace PeliculasApi.Tests.PruebasDeIntegracion
{
    [TestClass]
    public class GenerosControllerTests : BasePruebas
    {
        private static readonly string url = "/api/generos";

        [TestMethod]
        public async Task ObtenerTodosLosGenerosListadoVacio()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreBD);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode(); //Asegurarnos que tenemos una respuesta éxitosa

            var generos = JsonConvert
                .DeserializeObject<List<GeneroDTO>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(0, generos.Count);
        }

        [TestMethod]
        public async Task ObtenerTodosLosGeneros()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreBD);

            var contexto = ConstruirContext(nombreBD);
            contexto.Generos.Add(new Genero() { Nombre = "Género 1" });
            contexto.Generos.Add(new Genero() { Nombre = "Género 2" });
            await contexto.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            var generos = JsonConvert
                .DeserializeObject<List<GeneroDTO>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(2, generos.Count);
        }

        [TestMethod]
        public async Task BorrarGeneros()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreBD); //ConstruirWebApplicationFactory(nombreBD, true) El true indica que se va a saltar las medidas de seguridad

            var contexto = ConstruirContext(nombreBD);
            contexto.Generos.Add(new Genero() { Nombre = "Género 1" });
            await contexto.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.DeleteAsync($"{url}/1");
            respuesta.EnsureSuccessStatusCode();

            var contexto2 = ConstruirContext(nombreBD);
            var existe = await contexto2.Generos.AnyAsync();

            Assert.IsFalse(existe);
        }

        [TestMethod]
        public async Task BorrarGenerosRetorna401()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.DeleteAsync($"{url}/1");

            Assert.AreEqual("Unauthorized", respuesta.ReasonPhrase);
        }
    }
}
