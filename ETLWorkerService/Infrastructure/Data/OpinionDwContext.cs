
using Microsoft.EntityFrameworkCore;

namespace ETLWorkerService.Infrastructure.Data
{
    public class OpinionDwContext : DbContext
    {
        public OpinionDwContext(DbContextOptions<OpinionDwContext> options) : base(options)
        {
        }

        public DbSet<DimFecha> DimFechas { get; set; }
        public DbSet<DimCliente> DimClientes { get; set; }
        public DbSet<DimProducto> DimProductos { get; set; }
        public DbSet<DimFuente> DimFuentes { get; set; }
        public DbSet<DimClasificacion> DimClasificaciones { get; set; }
        public DbSet<FactOpiniones> FactOpiniones { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)

                {

                    modelBuilder.Entity<DimFecha>().ToTable("DimFecha");

                    modelBuilder.Entity<DimFecha>().HasKey(df => df.FechaKey);

                    modelBuilder.Entity<DimCliente>().ToTable("DimCliente");

                    modelBuilder.Entity<DimCliente>().HasKey(dc => dc.ClienteKey);

                    modelBuilder.Entity<DimProducto>().ToTable("DimProducto");

                    modelBuilder.Entity<DimProducto>().HasKey(dp => dp.ProductoKey);

                    modelBuilder.Entity<DimFuente>().ToTable("DimFuente");

                    modelBuilder.Entity<DimFuente>().HasKey(df => df.FuenteKey);

                    modelBuilder.Entity<DimClasificacion>().ToTable("DimClasificacion");

                    modelBuilder.Entity<DimClasificacion>().HasKey(dc => dc.ClasificacionKey);

        

                    modelBuilder.Entity<FactOpiniones>().ToTable("FactOpiniones");

                                modelBuilder.Entity<FactOpiniones>()

                                    .HasKey(fo => fo.OpinionKey);

                }
    }

    public class DimFecha
    {
        public int FechaKey { get; set; }
        public DateTime Fecha { get; set; }
        public int Dia { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int Trimestre { get; set; }
        public int DiaDeLaSemana { get; set; }
    }

    public class DimCliente
    {
        public int ClienteKey { get; set; }
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
    }

    public class DimProducto
    {
        public int ProductoKey { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public string? NombreCategoria { get; set; }
    }

    public class DimFuente
    {
        public int FuenteKey { get; set; }
        public string? NombreFuente { get; set; }
    }

    public class DimClasificacion
    {
        public int ClasificacionKey { get; set; }
        public string? NombreClasificacion { get; set; }
    }

    public class FactOpiniones
    {
        public long OpinionKey { get; set; }
        public int FechaKey { get; set; }
        public int ClienteKey { get; set; }
        public int ProductoKey { get; set; }
        public int FuenteKey { get; set; }
        public int? ClasificacionKey { get; set; }
        public int? Rating { get; set; }
        public int? PuntajeSatisfaccion { get; set; }
        public decimal? SentimentScore { get; set; }
    }
}
