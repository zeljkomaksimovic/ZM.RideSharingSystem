using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZM.NotificationService.Api.Application.UseCases.SendPaymentReceiptNotification;
using ZM.RideSharingSystem.Contracts.Commands.Notification;
using ZM.NotificationService.Api.Persistence;

namespace ZM.NotificationService.Api.Infrastructure.Consumers
{
    public class PaymentReceiptConsumer : IConsumer<PaymentReceiptNotificationCommand>
    {
        private readonly ILogger<PaymentReceiptConsumer> _logger;
        private readonly ISender _sender;
        private readonly NotificationDbContext _dbContext;

        public PaymentReceiptConsumer(ILogger<PaymentReceiptConsumer> logger, ISender sender, NotificationDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<PaymentReceiptNotificationCommand> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(PaymentReceiptConsumer),
                    context.CancellationToken))
            {
                return;
            }

            try
            {
                await _dbContext.ProcessedMessages.AddAsync(new Persistence.Idempotence.ProcessedMessage
                {
                    MessageId = context.MessageId,
                    ConsumerName = nameof(PaymentReceiptConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _dbContext.SaveChangesAsync(context.CancellationToken);

                await _sender.Send(new SendPaymentReceiptNotificationCommand(
                    context.Message.PaymentId,
                    context.Message.RideId,
                    context.Message.Amount,
                    context.Message.RecipientEmail),
                    context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {nameof(PaymentReceiptConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
