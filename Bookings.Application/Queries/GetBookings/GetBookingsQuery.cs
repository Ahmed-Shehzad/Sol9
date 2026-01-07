using Bookings.Application.Contexts;
using Bookings.Application.Dtos;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Sol9.Core.Pagination;

using Verifier;

namespace Bookings.Application.Queries.GetBookings;

public sealed record GetBookingsQuery(
    string CacheKey,
    int Page,
    int PageSize,
    TimeSpan? CacheDuration = null) : ICachedQuery<PagedResult<BookingDto>>;

public sealed class GetBookingsQueryValidator : AbstractValidator<GetBookingsQuery>
{
    public GetBookingsQueryValidator()
    {
        _ = RuleFor(request => request.Page)
            .Must(page => page >= 1, "Page must be greater than or equal to 1.");
        _ = RuleFor(request => request.PageSize)
            .Must(
                size => size >= 1 && size <= PaginationRequest.MaxPageSize,
                $"PageSize must be between 1 and {PaginationRequest.MaxPageSize}.");
    }
}

public sealed class GetBookingsQueryHandler : IQueryHandler<GetBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IReadOnlyBookingsDbContext _context;

    public GetBookingsQueryHandler(IReadOnlyBookingsDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BookingDto>> HandleAsync(GetBookingsQuery request, CancellationToken cancellationToken = default)
    {
        IQueryable<Domain.Entities.Booking> query = _context.Bookings
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .AsQueryable();

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        IReadOnlyList<BookingDto> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(booking => new BookingDto(
                booking.Id.ToGuid(),
                booking.OrderId.ToGuid(),
                booking.CustomerName,
                booking.Status,
                booking.CreatedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<BookingDto>(items, totalCount, request.Page, request.PageSize);
    }
}
