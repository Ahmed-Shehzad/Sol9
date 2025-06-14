using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Transponder.Storage.EntityFrameworkCore.Interceptors;

public class UpdateInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // Apply update logic to entities marked for update
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        // Apply update logic to entities marked for update
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Applies update logic to entities marked for update in the current <see cref="DbContext"/>.
    /// Changes the state of each modified <see cref="Core.Types.AuditableEntity{Ulid}"/> to <see cref="EntityState.Modified"/>
    /// and calls the <c>Update</c> method on the entity.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> instance to process. If <c>null</c>, the method does nothing.</param>
    private static void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<Core.Types.AuditableEntity<Ulid>>().ToList();
        var modifiedEntries = entries.Where(entry => entry.State == EntityState.Modified);
        foreach (var entry in modifiedEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.Update();
        }
    }
}