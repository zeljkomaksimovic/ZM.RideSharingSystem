using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.AddAvailableDriver;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Presentation.Consumers
{
    public class DriverAvailableConsumer : IConsumer<DriverAvailableEvent>
    {
        private readonly IMediator _mediator;

        public DriverAvailableConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DriverAvailableEvent> context)
        {
            await _mediator.Send(new AddAvailableDriverCommand(
                context.Message.DriverId,
                context.Message.Latitude,
                context.Message.Longitude),
                context.CancellationToken);
        }
    }
}