using System.Reflection;
using BuildingBlocks.Contracts.Types;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class DbContextExtensions
    {
        private static string? GetDefaultSchema(MemberInfo type)
        {
            return type.Name.Replace(nameof(DbContext), string.Empty).ToFirstLetterLowerCase();
        }

        public static string? GetDefaultSchema<TContext>() where TContext: DbContext
        {
            return GetDefaultSchema(typeof(TContext));
        }
        
        public static string? GetDefaultSchema(this DbContext context)
        {
            return GetDefaultSchema(context.GetType());
        }

        /// <summary>
        ///     Auto assign tenant to entity state.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="tenantId">TenantId</param>
        public static void AddAutoAssignTenant(this DbContext dbContext, Ulid tenantId)
        {
            var entries = dbContext.ChangeTracker.Entries().Where(e =>
                e.Entity is ITenantDependent);
            
            foreach (var entry in entries)
            {
                ((ITenantDependent) entry.Entity).TenantId = tenantId;
            }
        }

        /// <summary>
        ///     Auto assign user to entity state.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="userId">UserId</param>
        public static void AddAutoAssignUser(this DbContext dbContext, Ulid? userId)
        {
            if (!userId.HasValue) return;
            
            var entries = dbContext.ChangeTracker.Entries().Where(e =>
                e.Entity is IUserDependent);
            
            foreach (var entry in entries)
            {
                ((IUserDependent) entry.Entity).UserId = userId.Value;
            }
        }
    }