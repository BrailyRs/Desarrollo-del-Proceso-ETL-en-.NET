using ETLWorkerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETLWorkerService.Infrastructure.Data.Contexts
{
    public class OpinionRContext : DbContext
    {
        public OpinionRContext(DbContextOptions<OpinionRContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Clasificacion> Clasificaciones { get; set; }
        public DbSet<ComentarioSocial> ComentariosSociales { get; set; }
        public DbSet<Encuesta> Encuestas { get; set; }
        public DbSet<WebReview> WebReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.IdCliente);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.IdProducto);
                entity.Ignore(e => e.Categoria);
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasKey(e => e.IdCategoria);
            });

            modelBuilder.Entity<Clasificacion>(entity =>
            {
                entity.ToTable("Clasificaciones");
                entity.HasKey(e => e.IdClasificacion);
            });

            modelBuilder.Entity<Fuente>(entity =>
            {
                entity.ToTable("Fuentes");
                entity.HasKey(e => e.IdFuente);
            });

            modelBuilder.Entity<ComentarioSocial>(entity =>
            {
                entity.ToTable("Comentarios");
                entity.HasKey(e => new { e.IdComentario, e.Fecha });
                entity.Property(e => e.IdComentario).HasColumnName("IdComment");
                entity.Property(e => e.IdFuente).HasColumnName("IdFuente");
                entity.Property(e => e.SentimentScore).HasColumnName("SentimientoScore");
                entity.Ignore(e => e.Fuente);
            });

            modelBuilder.Entity<Encuesta>(entity =>
            {
                entity.ToTable("Encuestas");
                entity.HasKey(e => new { e.IdEncuesta, e.Fecha });
                entity.Property(e => e.IdEncuesta).HasColumnName("IdOpinion");
                entity.Property(e => e.SentimentScore).HasColumnName("SentimientoScore");
                entity.Ignore(e => e.Fuente);
                entity.Ignore(e => e.Clasificacion);
            });

            modelBuilder.Entity<WebReview>(entity =>
            {
                entity.ToTable("WebReviews");
                entity.HasKey(e => new { e.IdReview, e.Fecha });
                entity.Property(e => e.SentimentScore).HasColumnName("SentimientoScore");
                entity.Ignore(e => e.Fuente);
            });
        }
    }
}
