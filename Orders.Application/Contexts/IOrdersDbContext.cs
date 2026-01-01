using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Orders.Domain.Entities;

namespace Orders.Application.Contexts;

public interface IOrdersDbContext
{
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
