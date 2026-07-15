using MassTransit;
using MediatR;
using ZM.NotificationService.Api.Application.UseCases.SendRideCompletedNotification;
using ZM.RideSharingSystem.Contracts.Commands.Notification;

namespace ZM.NotificationService.Api.Infrastructure.Consumers
{
    public class RideCompletedConsumer : IConsumer<RideCompletedNotificationCommand>
    {
        private readonly ISender _sender;

        public RideCompletedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<RideCompletedNotificationCommand> context)
        {
            await _sender.Send(new SendRideCompletedNotificationCommand(
                context.Message.RideId, 
                context.Message.RecipientEmail));
        }
    }
}
