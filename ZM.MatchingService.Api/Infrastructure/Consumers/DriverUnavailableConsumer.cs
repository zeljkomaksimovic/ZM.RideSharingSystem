using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.RemoveAvailableDriver;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Presentation.Consumers
{
    public class DriverUnavailableConsumer : IConsumer<DriverUnavailableEvent>
    {
        private readonly ISender _sender;

        public DriverUnavailableConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<DriverUnavailableEvent> context)
        {
            await _sender.Send(new RemoveAvailableDriverCommand(
                context.Message.DriverId),
                context.CancellationToken);
        }
    }
}