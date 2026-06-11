using Microsoft.EntityFrameworkCore;
using MassTransit;
using MediatR;
using ZM.RideService.Api.Application.UseCases.AssignDriver;
using ZM.RideService.Api.Persistence;
using ZM.RideSharingSystem.Contracts.Events;
using ZM.RideService.Api.Persistence.Idempotence;

namespace ZM.RideService.Api.Infrastructure.Consumers
{
    public class AssignDriverConsumer : IConsumer<DriverAssignedEvent>
    {
        private readonly ILogger<AssignDriverConsumer> _logger;
        private readonly ISender _sender;
        private readonly RideDbContext _dbContext;

        public AssignDriverConsumer(ILogger<AssignDriverConsumer> logger, ISender sender, RideDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<DriverAssignedEvent> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(AssignDriverConsumer),
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
                    ConsumerName = nameof(AssignDriverConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _sender.Send(new AssignDriverCommand(
                    context.Message.RideId,
                    context.Message.DriverId),
                    context.CancellationToken);

                //await _unitOfWork.CommitAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackAsync(context.CancellationToken);
                _logger.LogError(ex, $"Error processing {nameof(AssignDriverConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
