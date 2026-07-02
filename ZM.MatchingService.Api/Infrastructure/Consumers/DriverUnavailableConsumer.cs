using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.SetDriverUnavailable;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Presentation.Consumers
{
    public class DriverUnavailableConsumer : IConsumer<DriverUnavailableEvent>
    {
        private readonly IMediator _mediator;

        public DriverUnavailableConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DriverUnavailableEvent> context)
        {
            await _mediator.Send(new RemoveAvailableDriverCommand(
                context.Message.DriverId),
                context.CancellationToken);
        }
    }
}