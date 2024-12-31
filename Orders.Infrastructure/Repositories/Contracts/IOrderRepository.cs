using BuildingBlocks.Contracts.Types;
using Orders.Domain.Aggregates;

namespace Orders.Infrastructure.Repositories.Contracts;

public interface IOrderRepository : IRepository<Order>;