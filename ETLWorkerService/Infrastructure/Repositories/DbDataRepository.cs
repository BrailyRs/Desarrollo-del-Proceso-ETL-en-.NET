
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
            return await _context.Products
                .Include(p => p.Category)
                .Select(p => new Product
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    IdCategoria = p.IdCategoria,
                    Category = p.Category, // Include the navigation property
                    Categoria = p.Category != null ? p.Category.Nombre : null // Populate Categoria with the name
                })
                .ToListAsync();
        }

        public Task<IEnumerable<SocialComment>> GetSocialCommentsAsync()
        {
            // This method should be implemented if there are social comments in the database.
            return Task.FromResult(new List<SocialComment>().AsEnumerable());
        }

        public async Task<IEnumerable<Survey>> GetSurveysAsync()
        {
            return await _context.Surveys
                .Include(s => s.Classification)
                .Select(s => new Survey
                {
                    IdOpinion = s.IdOpinion,
                    IdCliente = s.IdCliente,
                    IdProducto = s.IdProducto,
                    Fecha = s.Fecha,
                    Comentario = s.Comentario,
                    IdClasificacion = s.IdClasificacion,
                    Classification = s.Classification, // Include the navigation property
                    Clasificacion = s.Classification != null ? s.Classification.Nombre : null, // Populate Clasificacion with the name
                    PuntajeSatisfaccion = s.PuntajeSatisfaccion
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            return await _context.WebReviews.ToListAsync();
        }
    }
}
