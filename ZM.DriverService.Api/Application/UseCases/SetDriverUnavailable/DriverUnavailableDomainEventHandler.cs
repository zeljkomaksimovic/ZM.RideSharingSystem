using MassTransit;
using MediatR;
using ZM.DriverService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.DriverService.Api.Application.UseCases.SetDriverUnavailable
{
    public class DriverLocationUpdatedDomainEventHandler : INotificationHandler<DriverUnavailableDomainEvent>
    {
        private readonly IBus _bus;

        public DriverLocationUpdatedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(DriverUnavailableDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new DriverUnavailableEvent(
                notification.DriverId),
                cancellationToken);
        }
    }
}
