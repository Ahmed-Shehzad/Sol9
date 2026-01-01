using System.Linq;

using Orders.Domain.Entities;

namespace Orders.Application.Contexts;

public interface IReadOnlyOrdersDbContext
{
    IQueryable<Order> Orders { get; }
}
