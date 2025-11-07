using Microsoft.Extensions.DependencyInjection;
using ETLWorkerService.Core.Interfaces;

namespace ETLWorkerService.Presentation
{
    public class ETLWorker : BackgroundService
    {
        private readonly ILogger<ETLWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ETLWorker(ILogger<ETLWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var etlService = scope.ServiceProvider.GetRequiredService<IETLService>();
                    await etlService.ExecuteAsync(stoppingToken);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}