using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Sol9.Core;

namespace Orders.Infrastructure.Interceptors;

public class AuditableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is not null) AuditableEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AuditableEntities(DbContext eventDataContext)
    {
        var entries = eventDataContext.ChangeTracker.Entries<BaseEntity>().ToList();

        foreach (EntityEntry<BaseEntity> entry in entries)
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.ApplyUpdateDateTime();
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.ApplySoftDelete();
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Added:
                default:
                    break;
            }
    }
}
