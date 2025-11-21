using ETLWorkerService.Application.Interfaces;
using ETLWorkerService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETLWorkerService.Application.Services
{
    public class ETLOrchestrator : IETLOrchestrator
    {
        private readonly ILogger<ETLOrchestrator> _logger;
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly IDataWarehouseRepository _dataWarehouseRepository;
        private readonly ITransformationService _transformationService;

        public ETLOrchestrator(
            ILogger<ETLOrchestrator> logger,
            IDataSourceRepository dataSourceRepository,
            IDataWarehouseRepository dataWarehouseRepository,
            ITransformationService transformationService)
        {
            _logger = logger;
            _dataSourceRepository = dataSourceRepository;
            _dataWarehouseRepository = dataWarehouseRepository;
            _transformationService = transformationService;
        }

        public async Task ExecuteETLProcessAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("=================================================================");
                _logger.LogInformation("INICIANDO PROCESO ETL AUTOMÁTICO");
                _logger.LogInformation("=================================================================");
                _logger.LogInformation("");

                // FASE 1: EXTRACT
                _logger.LogInformation("[FASE 1/4] EXTRACCIÓN DE DATOS");
                _logger.LogInformation("-----------------------------------------------------------------");
                
                var clientes = await _dataSourceRepository.GetClientesAsync();
                _logger.LogInformation($"  ✓ Clientes extraídos: {clientes.Count()}");
                
                var productos = await _dataSourceRepository.GetProductosAsync();
                _logger.LogInformation($"  ✓ Productos extraídos: {productos.Count()}");
                
                var comentarios = await _dataSourceRepository.GetComentariosSocialesAsync();
                _logger.LogInformation($"  ✓ Comentarios sociales extraídos: {comentarios.Count()}");
                
                var encuestas = await _dataSourceRepository.GetEncuestasAsync();
                _logger.LogInformation($"  ✓ Encuestas extraídas: {encuestas.Count()}");
                
                var reviews = await _dataSourceRepository.GetWebReviewsAsync();
                _logger.LogInformation($"  ✓ Web reviews extraídas: {reviews.Count()}");
                _logger.LogInformation("");

                // FASE 2: TRANSFORM
                _logger.LogInformation("[FASE 2/4] TRANSFORMACIÓN DE DATOS");
                _logger.LogInformation("-----------------------------------------------------------------");
                
                var dimFechas = _transformationService.TransformToDimFecha(comentarios, encuestas, reviews);
                _logger.LogInformation($"  ✓ DimFecha transformada: {dimFechas.Count()} registros");
                
                var dimClientes = _transformationService.TransformToDimCliente(clientes);
                _logger.LogInformation($"  ✓ DimCliente transformada: {dimClientes.Count()} registros");
                
                var dimProductos = _transformationService.TransformToDimProducto(productos);
                _logger.LogInformation($"  ✓ DimProducto transformada: {dimProductos.Count()} registros");
                
                var dimFuentes = _transformationService.TransformToDimFuente(comentarios, encuestas, reviews);
                _logger.LogInformation($"  ✓ DimFuente transformada: {dimFuentes.Count()} registros");
                
                var dimClasificaciones = _transformationService.TransformToDimClasificacion(encuestas);
                _logger.LogInformation($"  ✓ DimClasificacion transformada: {dimClasificaciones.Count()} registros");
                _logger.LogInformation("");

                // FASE 3: LOAD DIMENSIONS (con Bulk Insert)
                _logger.LogInformation("[FASE 3/4] CARGA DE DIMENSIONES (BULK INSERT)");
                _logger.LogInformation("-----------------------------------------------------------------");
                
                _logger.LogInformation("  Truncando tablas existentes...");
                await _dataWarehouseRepository.TruncateAllTablesAsync();
                _logger.LogInformation("  ✓ Tablas truncadas");
                
                _logger.LogInformation("  Insertando DimFecha...");
                await _dataWarehouseRepository.BulkInsertDimFechaAsync(dimFechas);
                _logger.LogInformation("  ✓ DimFecha cargada");
                
                _logger.LogInformation("  Insertando DimCliente...");
                await _dataWarehouseRepository.BulkInsertDimClienteAsync(dimClientes);
                _logger.LogInformation("  ✓ DimCliente cargada");
                
                _logger.LogInformation("  Insertando DimProducto...");
                await _dataWarehouseRepository.BulkInsertDimProductoAsync(dimProductos);
                _logger.LogInformation("  ✓ DimProducto cargada");
                
                _logger.LogInformation("  Insertando DimFuente...");
                await _dataWarehouseRepository.BulkInsertDimFuenteAsync(dimFuentes);
                _logger.LogInformation("  ✓ DimFuente cargada");
                
                _logger.LogInformation("  Insertando DimClasificacion...");
                await _dataWarehouseRepository.BulkInsertDimClasificacionAsync(dimClasificaciones);
                _logger.LogInformation("  ✓ DimClasificacion cargada");
                _logger.LogInformation("");

                // Obtener las claves de las dimensiones
                _logger.LogInformation("  Obteniendo claves de dimensiones...");
                var clienteKeys = await _dataWarehouseRepository.GetClienteKeysAsync();
                var productoKeys = await _dataWarehouseRepository.GetProductoKeysAsync();
                var fuenteKeys = await _dataWarehouseRepository.GetFuenteKeysAsync();
                var clasificacionKeys = await _dataWarehouseRepository.GetClasificacionKeysAsync();
                var fechaKeys = await _dataWarehouseRepository.GetFechaKeysAsync();
                _logger.LogInformation("  ✓ Claves obtenidas");
                _logger.LogInformation("");

                // FASE 4: LOAD FACTS (con Bulk Insert)
                _logger.LogInformation("[FASE 4/4] CARGA DE TABLA DE HECHOS (BULK INSERT)");
                _logger.LogInformation("-----------------------------------------------------------------");
                
                var factOpiniones = _transformationService.TransformToFactOpiniones(
                    comentarios, encuestas, reviews,
                    clienteKeys, productoKeys, fuenteKeys, clasificacionKeys, fechaKeys);
                
                _logger.LogInformation($"  Transformados: {factOpiniones.Count()} hechos");
                
                _logger.LogInformation("  Insertando FactOpiniones...");
                await _dataWarehouseRepository.BulkInsertFactOpinionesAsync(factOpiniones);
                _logger.LogInformation("  ✓ FactOpiniones cargada");
                _logger.LogInformation("");

                _logger.LogInformation("=================================================================");
                _logger.LogInformation("PROCESO ETL COMPLETADO EXITOSAMENTE");
                _logger.LogInformation($"Total de opiniones cargadas: {factOpiniones.Count()}");
                _logger.LogInformation("=================================================================");
                _logger.LogInformation("");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR CRÍTICO en el proceso ETL");
                throw;
            }
        }
    }
}
