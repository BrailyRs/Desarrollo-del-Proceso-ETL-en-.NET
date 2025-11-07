
using ETLWorkerService.Core.Entities;
using ETLWorkerService.Core.Interfaces;
using ETLWorkerService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class DbDataRepository : IDataRepository
    {
        private readonly OpinionRContext _context;

        public DbDataRepository(OpinionRContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetClientsAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public Task<IEnumerable<SocialComment>> GetSocialCommentsAsync()
        {
            // This method should be implemented if there are social comments in the database.
            return Task.FromResult(new List<SocialComment>().AsEnumerable());
        }

        public async Task<IEnumerable<Survey>> GetSurveysAsync()
        {
            return await _context.Surveys.ToListAsync();
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            return await _context.WebReviews.ToListAsync();
        }
    }
}
