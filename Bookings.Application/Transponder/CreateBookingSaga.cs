using Bookings.Application.Contracts;
using Bookings.Domain.Entities;

using Sol9.Contracts.Bookings;

using Transponder.Abstractions;
using Transponder.Persistence.Abstractions;

namespace Bookings.Application.Transponder;

public sealed class CreateBookingSagaState : ISagaState
{
    public Guid CorrelationId { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid OrderId { get; set; }
}

public sealed class CreateBookingSaga : ISagaMessageHandler<CreateBookingSagaState, CreateBookingRequest>
{
    private readonly IBookingsRepository _repository;

    public CreateBookingSaga(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(ISagaConsumeContext<CreateBookingSagaState, CreateBookingRequest> context)
    {
        CreateBookingRequest request = context.Message;
        Booking? existing = await _repository.GetByOrderIdAsync(request.OrderId, context.CancellationToken)
            .ConfigureAwait(false);

        string status;
        Guid bookingId;

        if (existing is null)
        {
            var booking = Booking.Create(request.OrderId, request.CustomerName);
            await _repository.AddAsync(booking, context.CancellationToken).ConfigureAwait(false);
            await _repository.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
            bookingId = booking.Id;
            status = booking.Status;
        }
        else
        {
            bookingId = existing.Id;
            status = existing.Status;
        }

        context.Saga.OrderId = request.OrderId;
        context.MarkCompleted();

        await context.RespondAsync(new CreateBookingResponse(bookingId, request.OrderId, status), context.CancellationToken)
            .ConfigureAwait(false);
    }
}
