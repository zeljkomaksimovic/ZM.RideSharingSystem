using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZM.RideService.Api.Application.UseCases.StartRide;
using ZM.RideService.Api.Persistence;
using ZM.RideService.Api.Persistence.Idempotence;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.RideService.Api.Infrastructure.Consumers
{
    public class RideStartedConsumer : IConsumer<RideStartedEvent>
    {
        private readonly ILogger<CompleteRideConsumer> _logger;
        private readonly ISender _sender;
        private readonly RideDbContext _dbContext;

        public RideStartedConsumer(ILogger<CompleteRideConsumer> logger, ISender sender, RideDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<RideStartedEvent> context)
        {
            

            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(RideStartedConsumer),
                    context.CancellationToken))
            {
                return;
            }

            try
            {
                //Note: Transaction's are not allowed in In-Memory transport, so we will not use them here.
                //await _unitOfWork.BeginTransactionAsync(context.CancellationToken);

                await _dbContext.ProcessedMessages.AddAsync(new ProcessedMessage
                {
                    MessageId = context.MessageId,
                    ConsumerName = nameof(RideStartedConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _sender.Send(new StartRideCommand(
                    context.Message.RideId),
                    context.CancellationToken);

                //await _unitOfWork.CommitAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackAsync(context.CancellationToken);
                _logger.LogError(ex, $"Error processing {nameof(RideStartedConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
