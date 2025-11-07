using ETLWorkerService.Infrastructure.Data;

namespace ETLWorkerService.Core.Entities
{
    public class Survey
    {
        public int IdOpinion { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
        public int? IdClasificacion { get; set; } // Foreign key to Classification
        public Classification? Classification { get; set; } // Navigation property
        public string? Clasificacion { get; set; } // This will be populated with Classification.Nombre
        public int PuntajeSatisfaccion { get; set; }
    }
}