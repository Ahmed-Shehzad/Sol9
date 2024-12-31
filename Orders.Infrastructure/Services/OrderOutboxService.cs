using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Infrastructure.Services;
using Orders.Infrastructure.Repositories.Contracts;
using Orders.Infrastructure.Services.Contracts;

namespace Orders.Infrastructure.Services;

public class OrderOutboxService(IOrderOutboxRepository outboxRepository, IUnitOfWork unitOfWork)
    : OutboxService(outboxRepository, unitOfWork), IOrderOutboxService;