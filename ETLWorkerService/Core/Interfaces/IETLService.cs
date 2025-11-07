
namespace ETLWorkerService.Core.Interfaces
{
    public interface IETLService
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
