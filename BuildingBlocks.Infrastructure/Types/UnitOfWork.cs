using BuildingBlocks.Contracts.Types;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Types;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork
    where TContext : DbContext
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    { 
        return await context.SaveChangesAsync(cancellationToken);
    }
}