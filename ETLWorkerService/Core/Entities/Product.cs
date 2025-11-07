using ETLWorkerService.Infrastructure.Data;

namespace ETLWorkerService.Core.Entities
{
    public class Product
    {
        public int IdProducto { get; set; }
        public string? Nombre { get; set; }
        public int IdCategoria { get; set; } // Foreign key to Category
        public Category? Category { get; set; } // Navigation property
        public string? Categoria { get; set; } // This will be populated with Category.Nombre
    }
}