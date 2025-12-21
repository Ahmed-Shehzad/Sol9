using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.EntityFramework;

namespace Transponder.Persistence.EntityFramework.Tests;

internal sealed class EntityFrameworkTestDbContext : TransponderDbContext
{
    public EntityFrameworkTestDbContext(DbContextOptions<EntityFrameworkTestDbContext> options)
        : base(options, new EntityFrameworkStorageOptions())
    {
    }
}
