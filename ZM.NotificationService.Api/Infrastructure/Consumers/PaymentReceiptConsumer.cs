using MassTransit;
using MediatR;
using ZM.NotificationService.Api.Application.UseCases.SendPaymentReceiptNotification;
using ZM.RideSharingSystem.Contracts.Commands.Notification;

namespace ZM.NotificationService.Api.Infrastructure.Consumers
{
    public class PaymentReceiptConsumer : IConsumer<PaymentReceiptNotificationCommand>
    {
        private readonly ISender _sender;

        public PaymentReceiptConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<PaymentReceiptNotificationCommand> context)
        {
            await _sender.Send(new SendPaymentReceiptNotificationCommand(
                context.Message.PaymentId,
                context.Message.RideId,
                context.Message.Amount,
                context.Message.RecipientEmail));
        }
    }
}
