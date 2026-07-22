using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Domain.ValueObjects;
using ZM.RideSharingSystem.Contracts.Commands.Matching;

namespace ZM.MatchingService.Api.Infrastructure.Consumers
{
    public class FindDriverConsumer : IConsumer<FindDriverCommand>
    {
        private readonly ISender _sender;

        public FindDriverConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<FindDriverCommand> context)
        {
            await _sender.Send(new Application.UseCases.FindDriver.FindDriverCommand(
                context.Message.RideId,
                new GeoLocation(
                    context.Message.Latitude,
                    context.Message.Longitude)));
        }
    }
}
