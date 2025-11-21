namespace ETLWorkerService.Application.DTOs
{
    // DTOs para las respuestas de la API que incluyen objetos anidados
    
    public class ClienteDTO
    {
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
    }

    public class ProductoDTO
    {
        public int IdProducto { get; set; }
        public string? Nombre { get; set; }
    }

    public class FuenteDTO
    {
        public int IdFuente { get; set; }
        public string? Nombre { get; set; }
    }

    public class ClasificacionDTO
    {
        public int IdClasificacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class ComentarioApiResponse
    {
        public string? IdComment { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
        public double? SentimientoScore { get; set; }
        public ClienteDTO? Cliente { get; set; }
        public ProductoDTO? Producto { get; set; }
        public FuenteDTO? Fuente { get; set; }
    }

    public class EncuestaApiResponse
    {
        public int IdOpinion { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
        public int? PuntajeSatisfaccion { get; set; }
        public double? SentimientoScore { get; set; }
        public ClienteDTO? Cliente { get; set; }
        public ProductoDTO? Producto { get; set; }
        public ClasificacionDTO? Clasificacion { get; set; }
    }

    public class WebReviewApiResponse
    {
        public string? IdReview { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
        public int? Rating { get; set; }
        public double? SentimientoScore { get; set; }
        public ClienteDTO? Cliente { get; set; }
        public ProductoDTO? Producto { get; set; }
    }
}
