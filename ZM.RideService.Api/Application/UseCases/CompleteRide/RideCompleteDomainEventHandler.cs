using MassTransit;
using MediatR;
using ZM.RideService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Commands.Notification;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.RideService.Api.Application.UseCases.CompleteRide
{
    public class RideCompleteDomainEventHandler : INotificationHandler<RideCompletedDomainEvent>
    {
        private readonly IBus _bus;

        public RideCompleteDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(RideCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new RideCompletedEvent(notification.RideId), cancellationToken);
            await _bus.Publish(new RideCompletedNotificationCommand(notification.RideId), cancellationToken);
        }
    }
}
