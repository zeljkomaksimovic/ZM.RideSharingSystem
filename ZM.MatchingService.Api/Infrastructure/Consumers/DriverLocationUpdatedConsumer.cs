using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.UpdateDriverLocation;
using ZM.MatchingService.Api.Domain.ValueObjects;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Presentation.Consumers
{
    public class DriverLocationUpdatedConsumer : IConsumer<DriverLocationUpdatedEvent>
    {
        private readonly ISender _sender;

        public DriverLocationUpdatedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<DriverLocationUpdatedEvent> context)
        {
            await _sender.Send(new UpdateDriverLocationCommand(
                context.Message.DriverId,
                new GeoLocation(
                    context.Message.Latitude, 
                    context.Message.Longitude)),
                context.CancellationToken);
        }
    }
}