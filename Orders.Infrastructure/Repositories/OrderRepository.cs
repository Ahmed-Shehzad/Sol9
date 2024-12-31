using BuildingBlocks.Infrastructure.Repositories;
using Orders.Domain.Aggregates;
using Orders.Infrastructure.Contexts;
using Orders.Infrastructure.Repositories.Contracts;

namespace Orders.Infrastructure.Repositories;

public class OrderRepository(OrdersDbContext context) : Repository<Order>(context), IOrderRepository;