using MassTransit;
using MediatR;
using ZM.NotificationService.Api.Application.UseCases.SendDriverAssignedNotification;
using ZM.RideSharingSystem.Contracts.Commands.Notification;

namespace ZM.NotificationService.Api.Infrastructure.Consumers
{
    public class DriverAssignedConsumer : IConsumer<DriverAssignedNotificationCommand>
    {
        private readonly ISender _sender;

        public DriverAssignedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<DriverAssignedNotificationCommand> context)
        {
            await _sender.Send(new SendDriverAssignedNotificationCommand(
                context.Message.RideId, 
                context.Message.DriverId, 
                context.Message.RecipientEmail));
        }
    }
}
