namespace ETLWorkerService.Domain.Entities
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
    }

    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string? Nombre { get; set; }
    }

    public class Producto
    {
        public int IdProducto { get; set; }
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public int? IdCategoria { get; set; }
    }

    public class Clasificacion
    {
        public int IdClasificacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class Fuente
    {
        public int IdFuente { get; set; }
        public string? Nombre { get; set; }
    }

    public class ComentarioSocial
    {
        public string? IdComentario { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public string? Fuente { get; set; }
        public int? IdFuente { get; set; }
        public DateTime Fecha { get; set; }
        public double? SentimentScore { get; set; }
    }

    public class Encuesta
    {
        public int IdEncuesta { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int? IdCarga { get; set; }
        public string? Fuente { get; set; }
        public DateTime Fecha { get; set; }
        public int? PuntajeSatisfaccion { get; set; }
        public string? Clasificacion { get; set; }
        public int? IdClasificacion { get; set; }
        public double? SentimentScore { get; set; }
    }

    public class WebReview
    {
        public string? IdReview { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public int? IdCarga { get; set; }
        public string? Fuente { get; set; }
        public DateTime Fecha { get; set; }
        public int? Rating { get; set; }
        public string? Comentario { get; set; }
        public double? SentimentScore { get; set; }
    }
}
