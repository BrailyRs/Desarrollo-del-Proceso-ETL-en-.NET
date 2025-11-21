using ETLWorkerService.Domain.Entities;
using ETLWorkerService.Domain.Interfaces;
using ETLWorkerService.Infrastructure.Data.BulkInsert;
using ETLWorkerService.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Configuration;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class DataWarehouseRepository : IDataWarehouseRepository
    {
        private readonly OpinionDwContext _context;
        private readonly string _connectionString;
        private readonly int _batchSize;

        public DataWarehouseRepository(OpinionDwContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("OpinionDwContext") 
                ?? throw new InvalidOperationException("Connection string not found");
            _batchSize = configuration.GetValue<int>("ETLConfiguration:BulkInsertBatchSize", 10000);
        }

        public async Task TruncateAllTablesAsync()
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Deshabilitar todas las restricciones de FK
            await ExecuteNonQueryAsync(connection, "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
            
            // Eliminar todos los datos en orden correcto (hijos primero, luego padres)
            await ExecuteNonQueryAsync(connection, "DELETE FROM FactOpiniones");
            await ExecuteNonQueryAsync(connection, "DELETE FROM DimClasificacion");
            await ExecuteNonQueryAsync(connection, "DELETE FROM DimFuente");
            await ExecuteNonQueryAsync(connection, "DELETE FROM DimProducto");
            await ExecuteNonQueryAsync(connection, "DELETE FROM DimCliente");
            await ExecuteNonQueryAsync(connection, "DELETE FROM DimFecha");
            
            // Resetear los contadores de identidad
            await ExecuteNonQueryAsync(connection, "DBCC CHECKIDENT ('DimCliente', RESEED, 0)");
            await ExecuteNonQueryAsync(connection, "DBCC CHECKIDENT ('DimProducto', RESEED, 0)");
            await ExecuteNonQueryAsync(connection, "DBCC CHECKIDENT ('DimFuente', RESEED, 0)");
            await ExecuteNonQueryAsync(connection, "DBCC CHECKIDENT ('DimClasificacion', RESEED, 0)");
            await ExecuteNonQueryAsync(connection, "DBCC CHECKIDENT ('FactOpiniones', RESEED, 0)");
            
            // Rehabilitar todas las restricciones de FK
            await ExecuteNonQueryAsync(connection, "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
        }

        private async Task ExecuteNonQueryAsync(Microsoft.Data.SqlClient.SqlConnection connection, string sql)
        {
            try
            {
                using var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
                command.CommandTimeout = 300;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                // Ignorar errores de DBCC CHECKIDENT si la tabla no tiene columna identity
            }
        }

        public async Task BulkInsertDimFechaAsync(IEnumerable<DimFecha> fechas)
        {
            await BulkInsertHelper.BulkInsertAsync(fechas, _connectionString, "DimFecha", _batchSize);
        }

        public async Task BulkInsertDimClienteAsync(IEnumerable<DimCliente> clientes)
        {
            await BulkInsertHelper.BulkInsertAsync(clientes, _connectionString, "DimCliente", _batchSize);
        }

        public async Task BulkInsertDimProductoAsync(IEnumerable<DimProducto> productos)
        {
            await BulkInsertHelper.BulkInsertAsync(productos, _connectionString, "DimProducto", _batchSize);
        }

        public async Task BulkInsertDimFuenteAsync(IEnumerable<DimFuente> fuentes)
        {
            await BulkInsertHelper.BulkInsertAsync(fuentes, _connectionString, "DimFuente", _batchSize);
        }

        public async Task BulkInsertDimClasificacionAsync(IEnumerable<DimClasificacion> clasificaciones)
        {
            await BulkInsertHelper.BulkInsertAsync(clasificaciones, _connectionString, "DimClasificacion", _batchSize);
        }

        public async Task BulkInsertFactOpinionesAsync(IEnumerable<FactOpiniones> opiniones)
        {
            await BulkInsertHelper.BulkInsertAsync(opiniones, _connectionString, "FactOpiniones", _batchSize);
        }

        public async Task<Dictionary<int, int>> GetClienteKeysAsync()
        {
            return await BulkInsertHelper.ExecuteQueryToDictionaryAsync<int, int>(
                _connectionString,
                "SELECT IdCliente, ClienteKey FROM DimCliente");
        }

        public async Task<Dictionary<int, int>> GetProductoKeysAsync()
        {
            return await BulkInsertHelper.ExecuteQueryToDictionaryAsync<int, int>(
                _connectionString,
                "SELECT IdProducto, ProductoKey FROM DimProducto");
        }

        public async Task<Dictionary<string, int>> GetFuenteKeysAsync()
        {
            return await BulkInsertHelper.ExecuteQueryToDictionaryAsync<string, int>(
                _connectionString,
                "SELECT NombreFuente, FuenteKey FROM DimFuente");
        }

        public async Task<Dictionary<string, int>> GetClasificacionKeysAsync()
        {
            return await BulkInsertHelper.ExecuteQueryToDictionaryAsync<string, int>(
                _connectionString,
                "SELECT NombreClasificacion, ClasificacionKey FROM DimClasificacion");
        }

        public async Task<Dictionary<DateTime, int>> GetFechaKeysAsync()
        {
            return await BulkInsertHelper.ExecuteQueryToDictionaryAsync<DateTime, int>(
                _connectionString,
                "SELECT Fecha, FechaKey FROM DimFecha");
        }
    }
}
