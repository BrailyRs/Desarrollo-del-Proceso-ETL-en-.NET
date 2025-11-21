using ETLWorkerService.Application.DTOs;
using ETLWorkerService.Application.Interfaces;
using ETLWorkerService.Application.Services;
using ETLWorkerService.Domain.Interfaces;
using ETLWorkerService.Infrastructure.Data.Contexts;
using ETLWorkerService.Infrastructure.Factories;
using ETLWorkerService.Infrastructure.Repositories;
using ETLWorkerService.Presentation;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// =============================================================================
// CONFIGURACIÓN
// =============================================================================
builder.Services.Configure<ETLConfiguration>(
    builder.Configuration.GetSection("ETLConfiguration"));

// =============================================================================
// DATABASE CONTEXTS
// =============================================================================
builder.Services.AddDbContext<OpinionDwContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OpinionDwContext")),
    ServiceLifetime.Scoped);

builder.Services.AddDbContext<OpinionRContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OpinionRContext")),
    ServiceLifetime.Scoped);

// =============================================================================
// DATA SOURCE REPOSITORIES (Strategy Pattern)
// =============================================================================
builder.Services.AddScoped<CsvDataSourceRepository>();
builder.Services.AddScoped<ApiDataSourceRepository>();
builder.Services.AddScoped<DbDataSourceRepository>();

// =============================================================================
// FACTORY PATTERN
// =============================================================================
builder.Services.AddScoped<DataSourceRepositoryFactory>();

// =============================================================================
// DATA WAREHOUSE REPOSITORY (con Bulk Insert)
// =============================================================================
builder.Services.AddScoped<IDataWarehouseRepository, DataWarehouseRepository>();

// =============================================================================
// APPLICATION SERVICES
// =============================================================================
builder.Services.AddScoped<ITransformationService, TransformationService>();
// Nota: ETLOrchestrator NO se registra aquí porque se crea manualmente en ETLWorker
// usando el Factory para el DataSourceRepository

// =============================================================================
// HTTP CLIENT (para API Repository)
// =============================================================================
builder.Services.AddHttpClient();

// =============================================================================
// WORKER SERVICE (Presentation Layer)
// =============================================================================
builder.Services.AddHostedService<ETLWorker>();

// =============================================================================
// LOGGING CONFIGURATION
// =============================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// =============================================================================
// BUILD AND RUN
// =============================================================================
var host = builder.Build();

await host.RunAsync();
