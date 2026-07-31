using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZM.NotificationService.Api.Application.UseCases.SendRideCompletedNotification;
using ZM.RideSharingSystem.Contracts.Commands.Notification;
using ZM.NotificationService.Api.Persistence;

namespace ZM.NotificationService.Api.Infrastructure.Consumers
{
    public class RideCompletedConsumer : IConsumer<RideCompletedNotificationCommand>
    {
        private readonly ILogger<RideCompletedConsumer> _logger;
        private readonly ISender _sender;
        private readonly NotificationDbContext _dbContext;

        public RideCompletedConsumer(ILogger<RideCompletedConsumer> logger, ISender sender, NotificationDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<RideCompletedNotificationCommand> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(RideCompletedConsumer),
                    context.CancellationToken))
            {
                return;
            }

            try
            {
                await _dbContext.ProcessedMessages.AddAsync(new Persistence.Idempotence.ProcessedMessage
                {
                    MessageId = context.MessageId,
                    ConsumerName = nameof(RideCompletedConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _dbContext.SaveChangesAsync(context.CancellationToken);

                await _sender.Send(new SendRideCompletedNotificationCommand(
                    context.Message.RideId,
                    context.Message.RecipientEmail),
                    context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {nameof(RideCompletedConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
