using ETLWorkerService.Application.DTOs;
using ETLWorkerService.Application.Interfaces;
using ETLWorkerService.Domain.Interfaces;
using ETLWorkerService.Infrastructure.Factories;
using Microsoft.Extensions.Options;

namespace ETLWorkerService.Presentation
{
    public class ETLWorker : BackgroundService
    {
        private readonly ILogger<ETLWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ETLConfiguration _configuration;

        public ETLWorker(
            ILogger<ETLWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<ETLConfiguration> configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("╔═══════════════════════════════════════════════════════════════════╗");
                _logger.LogInformation("║        ETL WORKER SERVICE - MODO AUTOMÁTICO                       ║");
                _logger.LogInformation("║        Arquitectura Onion + Bulk Insert + Patrones de Diseño     ║");
                _logger.LogInformation("╚═══════════════════════════════════════════════════════════════════╝");
                _logger.LogInformation("");

                if (!_configuration.RunOnStartup)
                {
                    _logger.LogInformation("RunOnStartup = false. El servicio está en espera.");
                    _logger.LogInformation("Cambia 'RunOnStartup' a true en appsettings.json para ejecutar automáticamente.");
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    return;
                }

                _logger.LogInformation($"Fuente de datos configurada: {_configuration.DataSource}");
                _logger.LogInformation($"Tamaño de batch para Bulk Insert: {_configuration.BulkInsertBatchSize}");
                _logger.LogInformation($"Modo de ejecución: {_configuration.ExecutionMode}");
                _logger.LogInformation("");

                using var scope = _serviceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<DataSourceRepositoryFactory>();
                var dataSourceRepository = factory.Create(_configuration.DataSource);
                
                var transformationService = scope.ServiceProvider.GetRequiredService<ITransformationService>();
                var dataWarehouseRepository = scope.ServiceProvider.GetRequiredService<IDataWarehouseRepository>();
                
                var orchestratorLogger = scope.ServiceProvider.GetRequiredService<ILogger<Application.Services.ETLOrchestrator>>();
                var orchestrator = new Application.Services.ETLOrchestrator(
                    orchestratorLogger,
                    dataSourceRepository,
                    dataWarehouseRepository,
                    transformationService);

                await orchestrator.ExecuteETLProcessAsync(stoppingToken);

                _logger.LogInformation("");
                _logger.LogInformation("╔═══════════════════════════════════════════════════════════════════╗");
                _logger.LogInformation("║        PROCESO ETL COMPLETADO - Servicio finalizado              ║");
                _logger.LogInformation("╚═══════════════════════════════════════════════════════════════════╝");
                _logger.LogInformation("");
                _logger.LogInformation("Presiona Ctrl+C para cerrar la aplicación.");
                
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio ETL detenido por el usuario.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el Worker Service");
                throw;
            }
        }
    }
}
