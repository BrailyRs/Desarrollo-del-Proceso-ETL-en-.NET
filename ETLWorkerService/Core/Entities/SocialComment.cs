namespace ETLWorkerService.Core.Entities
{
    public class SocialComment
    {
        public required string IdComment { get; set; }
        public required int IdCliente { get; set; }
        public required int IdProducto { get; set; }
        public string? Fuente { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
    }
}