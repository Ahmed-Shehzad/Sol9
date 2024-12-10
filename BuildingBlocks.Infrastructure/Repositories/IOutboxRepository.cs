using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;

namespace BuildingBlocks.Infrastructure.Repositories;

public interface IOutboxRepository : IRepository<Outbox>
{
    Task<List<Outbox>> FindAllProcessedMessagesAsync(CancellationToken cancellationToken = default);
}