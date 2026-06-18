using MassTransit;
using MediatR;
using ZM.DriverService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.DriverService.Api.Application.UseCases.AssignRide
{
    public class RideAssignedDomainEventHandler : INotificationHandler<RideAssignedDomainEvent>
    {
        private readonly IBus _bus;

        public RideAssignedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(RideAssignedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new DriverAssignedEvent(
                notification.RideId, 
                notification.DriverId), 
                cancellationToken);
        }
    }
}
