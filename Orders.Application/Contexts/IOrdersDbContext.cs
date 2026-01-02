using Microsoft.EntityFrameworkCore;

using Orders.Domain.Entities;

using Sol9.Core.Abstractions;

namespace Orders.Application.Contexts;

public interface IOrdersDbContext : IDbContext
{
    DbSet<Order> Orders { get; }
}
