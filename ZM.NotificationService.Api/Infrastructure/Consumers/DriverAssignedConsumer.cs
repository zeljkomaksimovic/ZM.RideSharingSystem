using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZM.NotificationService.Api.Application.UseCases.SendDriverAssignedNotification;
using ZM.RideSharingSystem.Contracts.Commands.Notification;
using ZM.NotificationService.Api.Persistence;

namespace ZM.NotificationService.Api.Infrastructure.Consumers
{
    public class DriverAssignedConsumer : IConsumer<DriverAssignedNotificationCommand>
    {
        private readonly ILogger<DriverAssignedConsumer> _logger;
        private readonly ISender _sender;
        private readonly NotificationDbContext _dbContext;

        public DriverAssignedConsumer(ILogger<DriverAssignedConsumer> logger, ISender sender, NotificationDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<DriverAssignedNotificationCommand> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(DriverAssignedConsumer),
                    context.CancellationToken))
            {
                return;
            }

            try
            {
                await _dbContext.ProcessedMessages.AddAsync(new Persistence.Idempotence.ProcessedMessage
                {
                    MessageId = context.MessageId,
                    ConsumerName = nameof(DriverAssignedConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _dbContext.SaveChangesAsync(context.CancellationToken);

                await _sender.Send(new SendDriverAssignedNotificationCommand(
                    context.Message.RideId,
                    context.Message.DriverId,
                    context.Message.RecipientEmail),
                    context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {nameof(DriverAssignedConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
