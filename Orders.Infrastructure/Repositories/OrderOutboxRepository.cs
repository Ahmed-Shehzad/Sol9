using BuildingBlocks.Infrastructure.Repositories;
using Orders.Infrastructure.Contexts;
using Orders.Infrastructure.Repositories.Contracts;

namespace Orders.Infrastructure.Repositories;

public class OrderOutboxRepository(OrdersDbContext context) : OutboxRepository<OrdersDbContext>(context), IOrderOutboxRepository;