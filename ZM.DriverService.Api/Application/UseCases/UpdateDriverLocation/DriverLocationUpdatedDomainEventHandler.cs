using MassTransit;
using MediatR;
using ZM.DriverService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.DriverService.Api.Application.UseCases.UpdateDriverLocation
{
    public class DriverLocationUpdatedDomainEventHandler : INotificationHandler<DriverLocationUpdatedDomainEvent>
    {
        private readonly IBus _bus;

        public DriverLocationUpdatedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(DriverLocationUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new DriverLocationUpdatedEvent(
                notification.DriverId,
                notification.Latitude,
                notification.Longitude),
                cancellationToken);
        }
    }
}
