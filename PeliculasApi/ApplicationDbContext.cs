using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.Entidades;

namespace PeliculasApi
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        //Configurando la migración para que asigne llaves compuestas a ambas tablas PeliculasActores PeliculasGeneros
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeliculasActores>()
                .HasKey(x => new { x.PeliculaId, x.ActorId });
            modelBuilder.Entity<PeliculasGeneros>()
                .HasKey(x => new { x.PeliculaId, x.GeneroId });

            base.OnModelCreating(modelBuilder); //importante no borrar esta línea de código
        }

        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<Pelicula> Peliculas { get; set;}
        public DbSet<PeliculasActores> PeliculasActores { get; set; }
        public DbSet<PeliculasGeneros> PeliculasGeneros { get; set; }

    }
}
