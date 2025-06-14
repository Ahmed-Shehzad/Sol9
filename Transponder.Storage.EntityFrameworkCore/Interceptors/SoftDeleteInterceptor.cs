using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Transponder.Storage.EntityFrameworkCore.Interceptors;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // Apply soft delete logic to entities marked for deletion
        SoftDeleteEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        // Apply soft delete logic to entities marked for deletion
        SoftDeleteEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Applies soft delete logic to entities marked for deletion in the current <see cref="DbContext"/>.
    /// Changes the state of each deleted <see cref="Core.Types.SoftDeletableEntity{Ulid}"/> to <see cref="EntityState.Modified"/>
    /// and calls the <c>Delete</c> method on the entity.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> instance to process. If <c>null</c>, the method does nothing.</param>
    private static void SoftDeleteEntities(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<Core.Types.SoftDeletableEntity<Ulid>>().ToList();
        var deletedEntries = entries.Where(entry => entry.State == EntityState.Deleted);
        foreach (var entry in deletedEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.Delete();
        }
    }
}