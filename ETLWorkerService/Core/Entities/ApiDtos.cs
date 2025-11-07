using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ETLWorkerService.Core.Entities
{
    public class ClienteApiDto
    {
        [JsonPropertyName("IdCliente")]
        public int IdCliente { get; set; }
        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; }
    }

    public class ProductoApiDto
    {
        [JsonPropertyName("IdProducto")]
        public int IdProducto { get; set; }
        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; }
    }

    public class FuenteApiDto
    {
        [JsonPropertyName("IdFuente")]
        public int IdFuente { get; set; }
        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; }
    }

    public class ComentarioApiDto
    {
        [JsonPropertyName("IdComment")]
        public required string IdComment { get; set; }
        [JsonPropertyName("Fecha")]
        public DateTime Fecha { get; set; }
        [JsonPropertyName("Comentario")]
        public string? Comentario { get; set; }
        [JsonPropertyName("cliente")]
        public required ClienteApiDto Cliente { get; set; }
        [JsonPropertyName("producto")]
        public required ProductoApiDto Producto { get; set; }
        [JsonPropertyName("fuente")]
        public required FuenteApiDto Fuente { get; set; }
    }
}
