using MassTransit;
using MediatR;
using ZM.RideService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Commands;

namespace ZM.RideService.Api.Application.UseCases.AssignDriver
{
    public class DriverAssignedDomainEventHandler : INotificationHandler<DriverAssignedDomainEvent>
    {
        private readonly IBus _bus;

        public DriverAssignedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(DriverAssignedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new DriverAssignedNotificationCommand(notification.RideId, notification.DriverId), cancellationToken);
        }
    }
}
