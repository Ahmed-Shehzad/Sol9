using Verifier;

namespace Bookings.Application.Queries.GetBookingByOrderId;

public sealed class GetBookingByOrderIdQueryValidator : AbstractValidator<GetBookingByOrderIdQuery>
{
    public GetBookingByOrderIdQueryValidator()
    {
        _ = RuleFor(query => query.OrderId)
            .Must(id => id != Guid.Empty, "OrderId must not be empty.");
    }
}
