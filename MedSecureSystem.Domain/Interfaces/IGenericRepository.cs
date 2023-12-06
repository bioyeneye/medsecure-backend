using MedSecureSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace MedSecureSystem.Domain.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id);
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int currentPage = 1, int pageSize = 10);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task SaveAsync();
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity?> FirstOrDefaultWithIncludeAsync(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        void RollbackTransaction();
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>>? filter = null);
        IQueryable<TEntity> Query();
        DbSet<TEntity> QueryEntity();
    }
}