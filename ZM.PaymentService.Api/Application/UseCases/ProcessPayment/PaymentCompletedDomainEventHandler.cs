using MassTransit;
using MediatR;
using ZM.PaymentService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.PaymentService.Api.Application.UseCases.ProcessPayment
{
    public class PaymentCompletedDomainEventHandler : INotificationHandler<PaymentCompletedDomainEvent>
    {
        private readonly IBus _bus;

        public PaymentCompletedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new PaymentCompletedEvent(
                notification.PaymentId,
                notification.RideId,
                notification.Amount,
                notification.RecipientEmail),
                cancellationToken);
        }
    }
}
