using MassTransit;
using MediatR;
using ZM.RideService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.RideService.Api.Application.UseCases.CreateRide
{
    public class RideCreatedDomainEventHandler : INotificationHandler<RideCreatedDomainEvent>
    {
        private readonly IBus _bus;

        public RideCreatedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(RideCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new RideCreatedEvent(
                notification.RideId, 
                notification.RiderId), 
                cancellationToken);
        }
    }
}
