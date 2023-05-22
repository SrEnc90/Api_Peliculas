using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeliculasApi.Controllers;
using PeliculasApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class CuentasControllerTests : BasePruebas
    {

        [TestMethod]
        public async Task CrearUsuario()
        {
            var nombreBD = Guid.NewGuid().ToString();
            await CrearUsuarioHelper(nombreBD);
            var context2 = ConstruirContext(nombreBD);
            var conteo = await context2.Users.CountAsync();
            Assert.AreEqual(1, conteo);
        }

        [TestMethod]
        public async Task UsuarioNoPuedeLoguearse()
        {
            var nombreBD = Guid.NewGuid().ToString();
            await CrearUsuarioHelper(nombreBD);

            var controller = ConstruirCuentasController(nombreBD);
            var userInfo = new UserInfo() { Email = "ejemplo@gmail.com", Password = "malPassword" };
            var respuesta = await controller.Login(userInfo);

            Assert.IsNull(respuesta.Value);
            var resultado = respuesta.Result as BadRequestObjectResult;
            Assert.IsNotNull(resultado);
        }

        [TestMethod]
        public async Task UsuarioPuedeLogearse()
        {
            var nombreBD = Guid.NewGuid().ToString();
            await CrearUsuarioHelper(nombreBD);

            var controller = ConstruirCuentasController(nombreBD);
            var userInfo = new UserInfo() { Email = "ejemplo@gmail.com", Password = "Aa123546!" };
            var respuesta = await controller.Login(userInfo);
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token);
        }

        //Método que nos va a ayudar a CrearUsuario y a validar el logueo.
        private async Task CrearUsuarioHelper(string nombreBD)
        {
            var cuentasController = ConstruirCuentasController(nombreBD);
            var userInfo = new UserInfo() { Email = "ejemplo@gmail.com", Password = "Aa123546!" };
            await cuentasController.CreateUser(userInfo);
        }

        //Método auxiliar que creamos aparte, para centralizarlo y no tener que repetir líneas de código en cada TestMethod
        private CuentasController ConstruirCuentasController(string nombreBD)
        {
            var contexto = ConstruirContext(nombreBD);
            var miUserStore = new UserStore<IdentityUser>(contexto);
            var userManager = TestUserManager(miUserStore);
            var mapper = ConfigurarAutoMapper();

            //Debemos crear un DefaultHttpContext para poder hacerle un mock a IAuthenticationService
            var httpContext = new DefaultHttpContext();
            MockAuth(httpContext);
            var signInManager = SetupSignInManager(userManager, httpContext);

            //Creamos un diccionario para simular un json web Token y pasarselo al IConfigurarion, podemos copiar la misma key del appsetting.json
            var miConfiguracion = new Dictionary<string, string>
            {
                {"jwt:key","FSDAFSDFJKLASJFDSKJFNFASDFKSADHFSANFASKHKLFASDFKJASDLFKSDAJFLJFLSADJFLJSADFNS1234654AFJSDHFKFHKFSHAJHFKSAHFDSAJKF" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(miConfiguracion)
                .Build();

            return new CuentasController(userManager, signInManager, configuration, contexto, mapper);
        }

        //Hacemos un mock IAuthenticationService para que la autenticación no sea 100% real, sino que la podamos usar de una manera sencilla
        private Mock<IAuthenticationService> MockAuth(HttpContext contexto)
        {
            var auth = new Mock<IAuthenticationService>();
            contexto.RequestServices = new ServiceCollection().AddSingleton(auth.Object).BuildServiceProvider(); //Registramos el auth en el servicio de injection de dependecias para simular una autorización
            return auth;
        }

        /*
         * Mock solo se utiliza con interfaces y UserManager SignInManager no son interfaces, son clases
         * En el código fuente de aspnetcore ubicado en github, existe métodos auxiliares que nos van a permitir crear instancias de
         * estas clases y así pasarselas al controlador de cuentas. Rutas:
         * https://github.com/dotnet/aspnetcore/blob/main/src/Identity/test/Shared/MockHelpers.cs
         * https://github.com/dotnet/aspnetcore/blob/main/src/Identity/test/Identity.Test/SignInManagerTest.cs
         */
        public static ILookupNormalizer MockLookupNormalizer()
        {
            var normalizerFunc = new Func<string, string>(i =>
            {
                if (i == null)
                {
                    return null;
                }
                else
                {
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(i)).ToUpperInvariant();
                }
            });
            var lookupNormalizer = new Mock<ILookupNormalizer>();
            lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
            lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
            return lookupNormalizer.Object;
        }
        //Con este método contruimos un UserManager
        public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());
            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, MockLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }

        //Con este método contruimos un SignInManager
        private static SignInManager<PocoUser> SetupSignInManager<PocoUser>(UserManager<PocoUser> manager, 
            HttpContext context, ILogger logger = null, IdentityOptions identityOptions = null, 
            IAuthenticationSchemeProvider schemeProvider = null) where PocoUser : class
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context);
            identityOptions = identityOptions ?? new IdentityOptions();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(identityOptions);
            var claimsFactory = new UserClaimsPrincipalFactory<PocoUser>(manager, options.Object);
            schemeProvider = schemeProvider ?? new Mock<IAuthenticationSchemeProvider>().Object;
            var sm = new SignInManager<PocoUser>(manager, contextAccessor.Object, claimsFactory, options.Object, null, schemeProvider, new DefaultUserConfirmation<PocoUser>());
            sm.Logger = logger ?? NullLogger<SignInManager<PocoUser>>.Instance;
            return sm;
        }

       
    }
}
