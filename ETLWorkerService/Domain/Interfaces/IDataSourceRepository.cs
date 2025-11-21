using ETLWorkerService.Domain.Entities;

namespace ETLWorkerService.Domain.Interfaces
{
    public interface IDataSourceRepository
    {
        Task<IEnumerable<Cliente>> GetClientesAsync();
        Task<IEnumerable<Producto>> GetProductosAsync();
        Task<IEnumerable<ComentarioSocial>> GetComentariosSocialesAsync();
        Task<IEnumerable<Encuesta>> GetEncuestasAsync();
        Task<IEnumerable<WebReview>> GetWebReviewsAsync();
    }
}
