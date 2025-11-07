using System;
using Microsoft.Extensions.DependencyInjection;
using ETLWorkerService.Core.Interfaces;
using Microsoft.Extensions.Configuration; // Added for configuration access

namespace ETLWorkerService.Presentation
{
    public class ETLWorker : BackgroundService
    {
        private readonly ILogger<ETLWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Func<IServiceProvider, string, IETLService> _etlServiceFactory; // Inject the factory

        public ETLWorker(ILogger<ETLWorker> logger, IServiceScopeFactory scopeFactory, Func<IServiceProvider, string, IETLService> etlServiceFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _etlServiceFactory = etlServiceFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                Console.WriteLine("\nSeleccione el origen de datos para la extracción:");
                Console.WriteLine("1. CSV");
                Console.WriteLine("2. API");
                Console.WriteLine("3. Base de Datos");
                Console.WriteLine("4. Salir");
                Console.Write("Ingrese su opción (1, 2, 3 o 4): ");

                string? choice = Console.ReadLine();
                string selectedDataSourceType = "";

                switch (choice)
                {
                    case "1":
                        selectedDataSourceType = "csv";
                        break;
                    case "2":
                        selectedDataSourceType = "api";
                        break;
                    case "3":
                        selectedDataSourceType = "db";
                        break;
                    case "4":
                        _logger.LogInformation("Shutting down worker.");
                        return; // Exit the worker
                    default:
                        Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                        await Task.Delay(1000, stoppingToken); // Wait a bit before re-showing menu
                        continue; // Skip to next iteration of the loop
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var scopedServiceProvider = scope.ServiceProvider;
                    try
                    {
                        var etlService = _etlServiceFactory(scopedServiceProvider, selectedDataSourceType);
                        await etlService.ExecuteAsync(stoppingToken);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "Error al crear el servicio ETL: {Message}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error durante la ejecución del proceso ETL.");
                    }
                }

                Console.WriteLine("\nProceso de ETL completado. Presione cualquier tecla para volver al menú...");
                Console.ReadKey(); // Wait for user input before re-showing menu
            }
        }
    }
}