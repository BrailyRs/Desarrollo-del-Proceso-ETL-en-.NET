namespace ETLWorkerService.Domain.Entities
{
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
        public double? SentimentScore { get; set; }
    }
}
