using PeliculasApi.Controllers;
using PeliculasApi.Entidades;
using PeliculasApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using PeliculasApi.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class ActoresControllerTests : BasePruebas
    {
        [TestMethod]
        public async Task ObtenerActoresPaginados()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            contexto.Actores.Add(new Actor() { Nombre = "Actor 1" });
            contexto.Actores.Add(new Actor() { Nombre = "Actor 2" });
            contexto.Actores.Add(new Actor() { Nombre = "Actor 3" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);
            //Al instanciar un objeto de ActoresController debemos pasarle como tercer parámetro el almacenador de archivos en el método Get de paginación no lo usamos por eso colocamos null
            var controller = new ActoresController(contexto2, mapper, null);

            /*
            * Sin la línea de abajo Lanza error debido a que el HttpContext es null, 
            * debido a que no estamos dentro de un contexto http, ya que estamos invocando el controllador directamente 
            * y luego se hace la invocación del método get. Estamos manipulando directamente los métodos sin ningún 
            * contexto http(sin hacer solicitudes http). Por lo tanto agregamos un DefaultHttpContext() al controlador
            */
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var pagina1 = await controller.Get(new PaginacionDTO() { Pagina = 1, CantidadRegistrosPorPagina = 2 });
            var actoresPagina1 = pagina1.Value;
            Assert.AreEqual(2, actoresPagina1.Count());

            /*En el método InsertarParametrosPaginacion dentro de la carpeta helpers, cada vez que se invoca se agrega una cabecera a la respuesta http,
             no se puede agregar dos veces la misma cabecera al mismo contexto, por lo que reseteamos o instanciamos un nuevo contexto*/
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var pagina2 = await controller.Get(new PaginacionDTO { Pagina = 2, CantidadRegistrosPorPagina = 2 });
            var actoresPagina2 = pagina2.Value;
            Assert.AreEqual(1, actoresPagina2.Count());

            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var pagina3 = await controller.Get(new PaginacionDTO() { Pagina = 3, CantidadRegistrosPorPagina = 2 });
            var actoresPagina3 = pagina3.Value; // Si no coloca el await en la línea de arriba no sale el .Value
            Assert.AreEqual(0, actoresPagina3.Count);
        }

        [TestMethod]
        public async Task CrearActorSinFoto()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            var actor = new ActorCreacionDTO() { Nombre = "Carlos", FechaNacimiento = DateTime.Now };

            /*
             * Utilizamos la librería Moq para crear una clase auxiliar al cuál le vamos a pasar como parámetros null,
             * xq no nos interesa ya q queremos hacer las pruebas unitarias sin almacenar la foto.
             */
            var mock = new Mock<IAlmacenadorArchivos>();
            mock.Setup(x => x.GuardarArchivo(null, null, null, null))//Ponemos null, xq este método ni siquiera se va a invocar
                .Returns(Task.FromResult("url")); //colocamos Task xq es asíncrono y retorna el string "url"

            var controller = new ActoresController(contexto, mapper, mock.Object);
            var respuesta = await controller.Post(actor);
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.AreEqual(201, resultado.StatusCode);

            var contexto2 = ConstruirContext(nombreBD);
            var listado = await contexto2.Actores.ToListAsync();
            Assert.AreEqual(1, listado.Count);

            Assert.IsNull(listado[0].Foto); // Estamos testeando pero sin guardar la foto, por lo que debe salir true el Assert.IsNull la foto del primer registro
            Assert.AreEqual(0, mock.Invocations.Count); // Nos aseguramos del que método GuardarArchivo() no fue invocado(asegurarnos del que mock ni siquiera no fue tocado)
        }

        [TestMethod]
        public async Task CrearActorConFoto()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            //No es necesario un imagen en si, basta con convertir un string en un arreglo de bytes que va a simular la imagen
            var content = Encoding.UTF8.GetBytes("Imagen de Prueba");
            var archivo = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "imagen.jpg");
            archivo.Headers = new HeaderDictionary();
            archivo.ContentType = "image/jpg";

            var actor = new ActorCreacionDTO()
            {
                Nombre = "nuevo actor",
                FechaNacimiento = DateTime.Now,
                Foto = archivo
            };

            var mock = new Mock<IAlmacenadorArchivos>();
            mock.Setup(x => x.GuardarArchivo(content, ".jpg", "actores", archivo.ContentType))
                .Returns(Task.FromResult("url"));

            var controller = new ActoresController(contexto, mapper, mock.Object);
            var respuesta = await controller.Post(actor);
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.AreEqual(201, resultado.StatusCode);

            var contexto2 = ConstruirContext(nombreBD);
            var listado = await contexto2.Actores.ToListAsync();
            Assert.AreEqual(1, listado.Count);

            Assert.AreEqual("url", listado[0].Foto);
            Assert.AreEqual(1, mock.Invocations.Count);
        }

        [TestMethod]
        public async Task PatchRetornar404SiActorNoExiste()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            var controller = new ActoresController(contexto, mapper, null);
            var patchDoc = new JsonPatchDocument<ActorPatchDTO>();
            var respuesta = await controller.Patch(1, patchDoc);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task PatchActualizarUnSoloCampo()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();

            var fechaNacimiento = DateTime.Now;
            var actor = new Actor() { Nombre = "Carlos", FechaNacimiento = fechaNacimiento };
            await contexto.AddAsync(actor);
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContext(nombreBD);
            var controller = new ActoresController(contexto2, mapper, null); //Colocamos null xq no nos interesa almacenar la imagen(ya no vamos a probar eso)

            /*En el método patch dentro de ActoresController se instancia y se trabaja con TryValidateModel que es un método del ControllerBase
             * por lo cuál debemos proveerle de dicho método a nuestro test para que pase o apruebe el validador
             */
            var objectValidator = new Mock<IObjectModelValidator>();
            /*Con el código de abajo indicamos que le vamos a mandar cualquier context, con cualquier Validation, con cualquier string,
             * con cualquier objeto va a aprobar la el Validate
             */
            objectValidator.Setup(x => x.Validate(
                It.IsAny<ActionContext>(), // It.IsAny significa que va hacer cualquier cosa (cualquier validation va ser actionContext)
                It.IsAny<ValidationStateDictionary>(), //It.IsAny sea cualquier Validation
                It.IsAny<string>(), //It.IsAny sea cualquier string
                It.IsAny<object>())
            );

            controller.ObjectValidator = objectValidator.Object;

            var patchDoc = new JsonPatchDocument<ActorPatchDTO>();
            //new Operation<ActorPatchDTO>("Operacion que vamos hacer", "campo del objeto que queremos cambiar ", null, "Nuevo valor que vamos a cambiar")
            patchDoc.Operations.Add(new Operation<ActorPatchDTO>("replace", "/Nombre", null, "Patricia"));
            var respuesta = await controller.Patch(1, patchDoc);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado.StatusCode);

            var contexto3 = ConstruirContext(nombreBD);
            var actorDB = await contexto3.Actores.FirstAsync();
            Assert.AreEqual("Patricia", actorDB.Nombre);
            Assert.AreEqual(fechaNacimiento, actorDB.FechaNacimiento);
        }
    }
}
