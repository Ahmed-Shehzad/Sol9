using System;

using Verifier;

namespace Orders.Application.Queries.GetOrderById;

public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        _ = RuleFor(query => query.Id)
            .Must(id => id != Guid.Empty, "Id must not be empty.");
    }
}
