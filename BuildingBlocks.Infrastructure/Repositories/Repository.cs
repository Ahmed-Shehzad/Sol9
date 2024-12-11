using System.Linq.Expressions;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Repositories;

public class Repository<TModel> : IRepository<TModel> where TModel : BaseEntity
{
    protected readonly DbContext Context;
    protected readonly DbSet<TModel> Set;

    protected Repository(DbContext dbContext)
    {
        Context = dbContext;
        Set = dbContext.Set<TModel>();
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
    public async Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    public async Task<ICollection<TModel>> FindAllAsync(int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 100 : pageSize;
        pageSize = pageSize >= 1000 ? 1000 : pageSize;
        
        var query = Set.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return await query.ToListAsync(cancellationToken);
    }
    public async Task<ICollection<TModel>> FindAllByAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Set.Where(predicate).ToListAsync(cancellationToken);
    }
    public async Task<bool> AnyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Set.Where(predicate).AnyAsync(cancellationToken);
    }

    public void Add(TModel entity)
    {
        Set.Add(entity);
    }

    public void AddRange(IEnumerable<TModel> entities)
    {
        Set.AddRange(entities);
    }

    public void Update(TModel entity)
    {
        Set.Update(entity);
    }

    public void UpdateRange(IEnumerable<TModel> entities)
    {
        Set.UpdateRange(entities);
    }

    public void Remove(TModel entity)
    {
        Set.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TModel> entities)
    {
        Set.RemoveRange(entities);
    }
}