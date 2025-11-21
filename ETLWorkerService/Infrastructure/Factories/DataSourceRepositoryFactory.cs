using ETLWorkerService.Domain.Interfaces;
using ETLWorkerService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ETLWorkerService.Infrastructure.Factories
{
    public class DataSourceRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DataSourceRepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDataSourceRepository Create(string dataSourceType)
        {
            return dataSourceType.ToLower() switch
            {
                "csv" => _serviceProvider.GetRequiredService<CsvDataSourceRepository>(),
                "api" => _serviceProvider.GetRequiredService<ApiDataSourceRepository>(),
                "db" or "database" => _serviceProvider.GetRequiredService<DbDataSourceRepository>(),
                _ => throw new ArgumentException($"Tipo de fuente de datos desconocido: {dataSourceType}")
            };
        }
    }
}
