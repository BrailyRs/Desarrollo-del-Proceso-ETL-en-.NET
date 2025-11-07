
using ETLWorkerService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETLWorkerService.Infrastructure.Data
{
    public class OpinionRContext : DbContext
    {
        public OpinionRContext(DbContextOptions<OpinionRContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Classification> Classifications { get; set; }
        public DbSet<LoadRegister> LoadRegisters { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<WebReview> WebReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().ToTable("Clientes").HasKey(c => c.IdCliente);
            modelBuilder.Entity<Product>().ToTable("Productos").HasKey(p => p.IdProducto);
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.IdCategoria);
            modelBuilder.Entity<Category>().ToTable("Categorias").HasKey(c => c.IdCategoria);
            modelBuilder.Entity<Classification>().ToTable("Clasificaciones").HasKey(c => c.IdClasificacion);
            modelBuilder.Entity<LoadRegister>().ToTable("RegistroCargas").HasKey(lr => lr.IdCarga);
            modelBuilder.Entity<Source>().ToTable("Fuentes").HasKey(s => s.IdFuente);
            modelBuilder.Entity<Comment>().ToTable("Comentarios").HasKey(c => c.IdComment);
            modelBuilder.Entity<Survey>().ToTable("Encuestas").HasKey(s => s.IdOpinion);
            modelBuilder.Entity<Survey>()
                .HasOne(s => s.Classification)
                .WithMany()
                .HasForeignKey(s => s.IdClasificacion);
            modelBuilder.Entity<WebReview>().ToTable("WebReviews").HasKey(wr => wr.IdReview);
        }
    }

    public class Category
    {
        public int IdCategoria { get; set; }
        public string? Nombre { get; set; }
    }

    public class Classification
    {
        public int IdClasificacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class LoadRegister
    {
        public int IdCarga { get; set; }
        public string? Nombre { get; set; }
        public DateTime FechaCarga { get; set; }
    }

    public class Source
    {
        public int IdFuente { get; set; }
        public string? Nombre { get; set; }
    }

    public class Comment
    {
        public string? IdComment { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int IdFuente { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
    }
}
