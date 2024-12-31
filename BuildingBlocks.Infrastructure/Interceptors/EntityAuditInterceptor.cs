using BuildingBlocks.Contracts.Constants;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Utilities.Exceptions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

public class EntityAuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        try
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new DatabaseUpdateException(
                "Something went wrong while commiting changes to database.", new Error(exception.Unwrap()?.ToString()));
        }
    }

    private static void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var timestamp = TimeOnly.FromDateTime(utcNow);
        var entities = context.ChangeTracker.Entries<IAuditInfo>().ToList();
        foreach (var entry in entities)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.CreatedDateUtcAt), today);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.CreatedTimeUtcAt), timestamp);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.CreatedBy), Configurations.System);
                    break;
                case EntityState.Modified when !entry.Entity.IsDeleted:
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.UpdatedDateUtcAt), today);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.UpdatedTimeUtcAt), timestamp);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.UpdatedBy), Configurations.System);
                    break;
                case EntityState.Deleted when !entry.Entity.IsDeleted:
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.DeletedDateUtcAt), today);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.DeletedTimeUtcAt), timestamp);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.IsDeleted), true);
                    SetCurrentPropertyValue(entry, nameof(IAuditInfo.DeletedBy), Configurations.System);
                    entry.State = EntityState.Modified;
                    break;
            }
        }
    }

    private static void SetCurrentPropertyValue(EntityEntry entry, string propertyName, DateOnly utcNow)
    {
        entry.Property(propertyName).CurrentValue = utcNow;
    }
    
    private static void SetCurrentPropertyValue(EntityEntry entry, string propertyName, TimeOnly utcNow)
    {
        entry.Property(propertyName).CurrentValue = utcNow;
    }
    private static void SetCurrentPropertyValue(EntityEntry entry, string propertyName, string value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }

    private static void SetCurrentPropertyValue(EntityEntry entry, string propertyName, bool value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }
}