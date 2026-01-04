using Bookings.Application.Contexts;
using Bookings.Domain.Entities;

using Sol9.Contracts.Bookings;

using Transponder.Abstractions;
using Transponder.Persistence.Abstractions;

namespace Bookings.Application.Transponder;

public sealed class CancelBookingSagaState : ISagaState
{
    public Guid CorrelationId { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid OrderId { get; set; }
}

public sealed class CancelBookingSaga : ISagaMessageHandler<CancelBookingSagaState, CancelBookingRequest>
{
    private readonly IBookingsRepository _repository;

    public CancelBookingSaga(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(ISagaConsumeContext<CancelBookingSagaState, CancelBookingRequest> context)
    {
        CancelBookingRequest request = context.Message;
        Booking booking = await _repository.GetAsync(
                                  b => b.OrderId == request.OrderId,
                                  context.CancellationToken)
                              .ConfigureAwait(false)
                          ?? Booking.Create(
                              request.OrderId,
                              request.CustomerName);

        booking.MarkCancelled();
        _repository.Update(booking);
        await _repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        context.Saga.OrderId = request.OrderId;
        context.MarkCompleted();

        await context.RespondAsync(new CancelBookingResponse(booking.Id, request.OrderId, (int)booking.Status), context.CancellationToken).ConfigureAwait(false);
    }
}
