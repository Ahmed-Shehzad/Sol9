using Bookings.Domain.Entities;

namespace Bookings.Application.Contracts;

public interface IBookingsRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
