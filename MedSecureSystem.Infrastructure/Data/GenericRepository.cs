using MedSecureSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace MedSecureSystem.Infrastructure.Data
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private MedSecureContext _context;
        private DbSet<TEntity> _dbSet;
        private IDbContextTransaction _currentTransaction;

        public GenericRepository(MedSecureContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int currentPage = 1, int pageSize = 10)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.AsNoTracking().Where(filter);
            }

            var count = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (currentPage > 0 && pageSize > 0)
            {
                query = query.Skip((currentPage - 1) * pageSize).Take(pageSize);
            }

            var items = await query.ToListAsync();

            return new PaginatedResult<TEntity>(items, count, currentPage, pageSize);
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.AsNoTracking().Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        public IQueryable<TEntity> Query() => _context.Set<TEntity>().AsQueryable();
        public DbSet<TEntity> QueryEntity() => _context.Set<TEntity>();

        public async Task<TEntity?> FirstOrDefaultWithIncludeAsync(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (include != null)
            {
                query = include(_dbSet);
            }
            return await query.FirstOrDefaultAsync(filter);
        }


        public async Task BeginTransactionAsync()
        {
            _currentTransaction ??= await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction?.CommitAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }

}
