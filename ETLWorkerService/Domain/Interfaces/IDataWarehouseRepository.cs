using ETLWorkerService.Domain.Entities;

namespace ETLWorkerService.Domain.Interfaces
{
    public interface IDataWarehouseRepository
    {
        Task TruncateAllTablesAsync();
        Task BulkInsertDimFechaAsync(IEnumerable<DimFecha> fechas);
        Task BulkInsertDimClienteAsync(IEnumerable<DimCliente> clientes);
        Task BulkInsertDimProductoAsync(IEnumerable<DimProducto> productos);
        Task BulkInsertDimFuenteAsync(IEnumerable<DimFuente> fuentes);
        Task BulkInsertDimClasificacionAsync(IEnumerable<DimClasificacion> clasificaciones);
        Task BulkInsertFactOpinionesAsync(IEnumerable<FactOpiniones> opiniones);
        Task<Dictionary<int, int>> GetClienteKeysAsync();
        Task<Dictionary<int, int>> GetProductoKeysAsync();
        Task<Dictionary<string, int>> GetFuenteKeysAsync();
        Task<Dictionary<string, int>> GetClasificacionKeysAsync();
        Task<Dictionary<DateTime, int>> GetFechaKeysAsync();
    }
}
