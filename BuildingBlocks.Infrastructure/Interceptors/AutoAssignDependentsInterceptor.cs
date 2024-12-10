using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Utilities.Exceptions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

public class AutoAssignDependentsInterceptor(Ulid? tenantId, Ulid? userId) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);
        
        AddAutoAssignTenant(eventData.Context, tenantId);
        AddAutoAssignUser(eventData.Context, userId);

        try
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new DatabaseUpdateException(
                "Something went wrong while commiting changes to database.", new Error(exception.Unwrap()?.ToString()));
        }
    }
    
    /// <summary>
    ///     Auto assign tenant to entity state.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tenantId">TenantId</param>
    private static void AddAutoAssignTenant(DbContext context, Ulid? tenantId)
    {
        if (!tenantId.HasValue) return;
            
        var entries = context.ChangeTracker.Entries().Where(e => e.Entity is ITenantDependent);
            
        foreach (var entry in entries) SetCurrentPropertyValue(entry, nameof(ITenantDependent.TenantId), tenantId.Value);
    }
    
    /// <summary>
    ///     Auto assign user to entity state.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="userId">UserId</param>
    private static void AddAutoAssignUser(DbContext dbContext, Ulid? userId)
    {
        if (!userId.HasValue) return;
            
        var entries = dbContext.ChangeTracker.Entries().Where(e => e.Entity is IUserDependent);
            
        foreach (var entry in entries) SetCurrentPropertyValue(entry, nameof(IUserDependent.UserId), userId.Value);
    }
    
    private static void SetCurrentPropertyValue(EntityEntry entry, string propertyName, Ulid value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }
}