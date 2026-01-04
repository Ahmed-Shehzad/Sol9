using Sol9.Core;

using Verifier;

namespace Orders.Domain.Events.Integration;

public record OrderCreatedIntegrationEvent(Guid Id) : IntegrationEvent(Id);

public class OrderCreatedIntegrationEventValidator : AbstractValidator<OrderCreatedIntegrationEvent>
{
    public OrderCreatedIntegrationEventValidator()
    {
        _ = RuleFor(x => x.Id).NotNull().NotEmpty("Order Id must not be empty");
    }
}
