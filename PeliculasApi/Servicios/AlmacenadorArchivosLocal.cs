using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace PeliculasApi.Servicios
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly IWebHostEnvironment env; //Con esto vamos a poder obtener la ruta dónde se encuentra nuestra carpeta wwwroot
        private readonly IHttpContextAccessor httpContextAccessor; //Con esto vamos a poder determinar el dominio en donde tenemos publicado nuestra webApi

        public AlmacenadorArchivosLocal(IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccesor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccesor;
        }

        public Task BorrarArchivo(string ruta, string contenedor)
        {
            if (ruta != null)
            {
                var nombreArchivo = Path.GetFileName(ruta);
                string directorioArchivo = Path.Combine(env.WebRootPath, contenedor, nombreArchivo);

                if (File.Exists(directorioArchivo))
                {
                    File.Delete(directorioArchivo);
                }

            }

            return Task.FromResult(0);
        }

        public async Task<string> EditarArchivo(byte[] contenido, string extension, string contenedor, string ruta, string contentType)
        {
            await BorrarArchivo(ruta, contenedor);
            return await GuardarArchivo(contenido, extension, contenedor, contentType);
        }

        public async Task<string> GuardarArchivo(byte[] contenido, string extension, 
            string contenedor, string contentType)
        {
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(env.WebRootPath, contenedor);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string ruta = Path.Combine(folder, nombreArchivo);
            await File.WriteAllBytesAsync(ruta, contenido);

            //El httpContextAccessor.HttpContext.Request.Scheme se refiere al dominio, si es http o https
            var urlActual = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";

            var urlParaBD = Path.Combine(urlActual, contenedor, nombreArchivo);

            return urlParaBD;
        }
    }
}
