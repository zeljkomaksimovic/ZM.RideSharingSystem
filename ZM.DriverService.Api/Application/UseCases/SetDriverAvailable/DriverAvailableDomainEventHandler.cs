using MassTransit;
using MediatR;
using ZM.DriverService.Api.Domain.Events;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.DriverService.Api.Application.UseCases.SetDriverAvailable
{
    public class DriverUnavailableDomainEventHandler : INotificationHandler<DriverAvailableDomainEvent>
    {
        private readonly IBus _bus;

        public DriverUnavailableDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(DriverAvailableDomainEvent notification, CancellationToken cancellationToken)
        {
            await _bus.Publish(new DriverAvailableEvent(
                notification.DriverId),
                cancellationToken);
        }
    }
}
