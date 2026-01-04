using Sol9.Core;

using Verifier;

namespace Orders.Domain.Events.Integration;

public record OrderCancelledIntegrationEvent(Guid Id, string CustomerName) : IntegrationEvent(Id);

public class OrderCancelledIntegrationEventValidator : AbstractValidator<OrderCancelledIntegrationEvent>
{
    public OrderCancelledIntegrationEventValidator()
    {
        _ = RuleFor(x => x.Id).NotNull().NotEmpty("Order Id must not be empty");
    }
}
