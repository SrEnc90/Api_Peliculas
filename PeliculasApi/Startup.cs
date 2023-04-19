using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.Servicios;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PeliculasApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Agregando el servicio de automapper a todo el proyecto
            services.AddAutoMapper(typeof(Startup));

            //Configurando e inyectando el servicio de almacenamiento en azure
            //services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();

            //Configurando e inyectando el servicio de almacenamiento de manera local
            services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
            services.AddHttpContextAccessor();

            //Agregando el servicio de DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers()
                    .AddNewtonsoftJson(); //Agregando el servicio de NewtonsoftJson

            services.AddEndpointsApiExplorer();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            //Configurando nuestra webApi para que se pueda ver el contenido estático haciendo click a la url. Va antes del app.UseRouting
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}
