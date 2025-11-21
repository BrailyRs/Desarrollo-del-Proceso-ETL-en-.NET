using ETLWorkerService.Domain.Entities;
using ETLWorkerService.Domain.Interfaces;
using ETLWorkerService.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class DbDataSourceRepository : IDataSourceRepository
    {
        private readonly OpinionRContext _context;
        private readonly ILogger<DbDataSourceRepository> _logger;

        public DbDataSourceRepository(OpinionRContext context, ILogger<DbDataSourceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Cliente>> GetClientesAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetProductosAsync()
        {
            var productos = await _context.Productos.ToListAsync();
            var categorias = await _context.Categorias
                .ToDictionaryAsync(c => c.IdCategoria, c => c.Nombre);

            return productos.Select(p =>
            {
                p.Categoria = p.IdCategoria.HasValue && categorias.ContainsKey(p.IdCategoria.Value) 
                    ? categorias[p.IdCategoria.Value] 
                    : "Sin Categoría";
                return p;
            }).ToList();
        }

        public async Task<IEnumerable<ComentarioSocial>> GetComentariosSocialesAsync()
        {
            var comentarios = await _context.ComentariosSociales.ToListAsync();
            var fuentes = await _context.Set<Fuente>()
                .ToDictionaryAsync(f => f.IdFuente, f => f.Nombre);

            return comentarios.Select(c =>
            {
                if (c.IdFuente.HasValue && fuentes.ContainsKey(c.IdFuente.Value))
                {
                    c.Fuente = fuentes[c.IdFuente.Value];
                }
                return c;
            }).ToList();
        }

        public async Task<IEnumerable<Encuesta>> GetEncuestasAsync()
        {
            var encuestas = await _context.Encuestas.ToListAsync();
            var clasificaciones = await _context.Clasificaciones
                .ToDictionaryAsync(c => c.IdClasificacion, c => c.Nombre);

            return encuestas.Select(e =>
            {
                e.Fuente = "Encuesta";
                if (e.IdClasificacion.HasValue && clasificaciones.ContainsKey(e.IdClasificacion.Value))
                {
                    e.Clasificacion = clasificaciones[e.IdClasificacion.Value];
                }
                return e;
            }).ToList();
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            var reviews = await _context.WebReviews.ToListAsync();
            return reviews.Select(r =>
            {
                r.Fuente = "Web";
                return r;
            }).ToList();
        }
    }
}
