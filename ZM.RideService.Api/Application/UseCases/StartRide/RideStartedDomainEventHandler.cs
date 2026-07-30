using MassTransit;
using MediatR;
using ZM.RideService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.RideService.Api.Application.UseCases.StartRide
{
    public class RideStartedDomainEventHandler : INotificationHandler<RideStartedDomainEvent>
    {
        private readonly IBus _bus;

        public RideStartedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(RideStartedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new RideStartedEvent(
                notification.RideId),
                cancellationToken);
        }
    }
}
