using ETLWorkerService.Domain.Interfaces;
using ETLWorkerService.Infrastructure.Data.BulkInsert;
using Microsoft.EntityFrameworkCore;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly string _connectionString;

        public GenericRepository(DbContext context, string connectionString)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _connectionString = connectionString;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public virtual async Task BulkInsertAsync(IEnumerable<T> entities, string tableName)
        {
            await BulkInsertHelper.BulkInsertAsync(entities, _connectionString, tableName);
        }
    }
}
