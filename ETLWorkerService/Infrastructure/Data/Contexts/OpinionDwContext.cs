using ETLWorkerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETLWorkerService.Infrastructure.Data.Contexts
{
    public class OpinionDwContext : DbContext
    {
        public OpinionDwContext(DbContextOptions<OpinionDwContext> options) : base(options) { }

        public DbSet<DimFecha> DimFecha { get; set; }
        public DbSet<DimCliente> DimCliente { get; set; }
        public DbSet<DimProducto> DimProducto { get; set; }
        public DbSet<DimFuente> DimFuente { get; set; }
        public DbSet<DimClasificacion> DimClasificacion { get; set; }
        public DbSet<FactOpiniones> FactOpiniones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DimFecha>(entity =>
            {
                entity.ToTable("DimFecha");
                entity.HasKey(e => e.FechaKey);
                entity.Property(e => e.FechaKey).ValueGeneratedNever();
            });

            modelBuilder.Entity<DimCliente>(entity =>
            {
                entity.ToTable("DimCliente");
                entity.HasKey(e => e.ClienteKey);
                entity.Property(e => e.ClienteKey).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<DimProducto>(entity =>
            {
                entity.ToTable("DimProducto");
                entity.HasKey(e => e.ProductoKey);
                entity.Property(e => e.ProductoKey).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<DimFuente>(entity =>
            {
                entity.ToTable("DimFuente");
                entity.HasKey(e => e.FuenteKey);
                entity.Property(e => e.FuenteKey).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<DimClasificacion>(entity =>
            {
                entity.ToTable("DimClasificacion");
                entity.HasKey(e => e.ClasificacionKey);
                entity.Property(e => e.ClasificacionKey).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<FactOpiniones>(entity =>
            {
                entity.ToTable("FactOpiniones");
                entity.HasKey(e => e.OpinionKey);
                entity.Property(e => e.OpinionKey).ValueGeneratedOnAdd();
            });
        }
    }
}
