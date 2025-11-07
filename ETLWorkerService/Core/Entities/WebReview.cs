namespace ETLWorkerService.Core.Entities
{
    public class WebReview
    {
        public required string IdReview { get; set; }
        public required int IdCliente { get; set; }
        public required int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
        public int Rating { get; set; }
    }
}