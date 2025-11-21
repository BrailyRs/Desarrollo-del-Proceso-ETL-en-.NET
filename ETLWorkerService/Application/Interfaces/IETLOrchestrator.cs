namespace ETLWorkerService.Application.Interfaces
{
    public interface IETLOrchestrator
    {
        Task ExecuteETLProcessAsync(CancellationToken cancellationToken = default);
    }
}
