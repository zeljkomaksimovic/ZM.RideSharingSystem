using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.AddAvailableDriver;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Presentation.Consumers
{
    public class DriverAvailableConsumer : IConsumer<DriverAvailableEvent>
    {
        private readonly ISender _sender;

        public DriverAvailableConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<DriverAvailableEvent> context)
        {
            await _sender.Send(new AddAvailableDriverCommand(
                context.Message.DriverId),
                context.CancellationToken);
        }
    }
}