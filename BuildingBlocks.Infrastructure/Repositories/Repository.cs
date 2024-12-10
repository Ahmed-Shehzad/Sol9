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
        ArgumentNullException.ThrowIfNull(dbContext);

        Context = dbContext;
        Set = dbContext.Set<TModel>();
    }

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
    public async Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    public async Task<ICollection<TModel>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await Set.ToListAsync(cancellationToken);
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