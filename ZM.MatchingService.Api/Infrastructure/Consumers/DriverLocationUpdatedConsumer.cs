using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.UpdateDriverLocation;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Presentation.Consumers
{
    public class DriverLocationUpdatedConsumer : IConsumer<DriverLocationUpdatedEvent>
    {
        private readonly IMediator _mediator;

        public DriverLocationUpdatedConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DriverLocationUpdatedEvent> context)
        {
            await _mediator.Send(new UpdateDriverLocationCommand(
                context.Message.DriverId,
                context.Message.Latitude,
                context.Message.Longitude),
                context.CancellationToken);
        }
    }
}