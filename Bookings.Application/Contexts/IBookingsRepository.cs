using Bookings.Domain.Entities;

using Sol9.Core.Abstractions;

namespace Bookings.Application.Contexts;

public interface IBookingsRepository : IRepository<Booking>;
