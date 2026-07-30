using Microsoft.EntityFrameworkCore;
using MassTransit;
using MediatR;
using ZM.DriverService.Api.Application.UseCases.CompleteRide;
using ZM.DriverService.Api.Persistence;
using ZM.DriverService.Api.Persistence.Idempotence;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.DriverService.Api.Infrastructure.Consumers
{
    public class DriverCompleteRideConsumer : IConsumer<DriverCompleteRideEvent>
    {
        private readonly ILogger<DriverCompleteRideConsumer> _logger;
        private readonly ISender _sender;
        private readonly DriverDbContext _dbContext;

        public DriverCompleteRideConsumer(ILogger<DriverCompleteRideConsumer> logger, ISender sender, DriverDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<DriverCompleteRideEvent> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(DriverCompleteRideConsumer),
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
                    ConsumerName = nameof(DriverCompleteRideConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _sender.Send(new CompleteRideCommand(
                    context.Message.DriverId),
                    context.CancellationToken);

                //await _unitOfWork.CommitAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackAsync(context.CancellationToken);
                _logger.LogError(ex, $"Error processing {nameof(DriverCompleteRideConsumer)} with MessageId: {context.MessageId}");
            }
            
        }
    }
}
