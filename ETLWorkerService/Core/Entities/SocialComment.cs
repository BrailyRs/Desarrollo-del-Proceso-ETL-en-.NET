namespace ETLWorkerService.Core.Entities
{
    public class SocialComment
    {
        public string IdComment { get; set; }
        public string IdCliente { get; set; }
        public string IdProducto { get; set; }
        public string? Fuente { get; set; }
        public DateTime Fecha { get; set; }
        public string? comentario { get; set; }
    }
}