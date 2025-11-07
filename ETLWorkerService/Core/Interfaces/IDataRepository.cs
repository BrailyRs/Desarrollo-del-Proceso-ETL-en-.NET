
using ETLWorkerService.Core.Entities;

namespace ETLWorkerService.Core.Interfaces
{
    public interface IDataRepository
    {
        Task<IEnumerable<Client>> GetClientsAsync();
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<IEnumerable<SocialComment>> GetSocialCommentsAsync();
        Task<IEnumerable<Survey>> GetSurveysAsync();
        Task<IEnumerable<WebReview>> GetWebReviewsAsync();
    }
}
