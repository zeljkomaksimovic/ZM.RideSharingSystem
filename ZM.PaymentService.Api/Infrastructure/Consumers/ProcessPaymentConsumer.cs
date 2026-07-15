using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZM.PaymentService.Api.Persistence;
using ZM.PaymentService.Api.Application.UseCases.ProcessPayment;

namespace ZM.PaymentService.Api.Infrastructure.Consumers
{
    public class ProcessPaymentConsumer : IConsumer<ProcessPaymentCommand>
    {
        private readonly ILogger<ProcessPaymentConsumer> _logger;
        private readonly ISender _sender;
        private readonly PaymentDbContext _dbContext;

        public ProcessPaymentConsumer(ILogger<ProcessPaymentConsumer> logger, ISender sender, PaymentDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(ProcessPaymentConsumer),
                    context.CancellationToken))
            {
                return;
            }

            try
            {
                //Note: Transaction's are not allowed in In-Memory transport, so we will not use them here.
                //await _unitOfWork.BeginTransactionAsync(context.CancellationToken);

                await _dbContext.ProcessedMessages.AddAsync(new Persistence.Idempotence.ProcessedMessage
                {
                    MessageId = context.MessageId,
                    ConsumerName = nameof(ProcessPaymentConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                await _sender.Send(new ProcessPaymentCommand(
                    context.Message.RideId,
                    context.Message.Amount,
                    context.Message.RecipientEmail), 
                    context.CancellationToken);

                //await _unitOfWork.CommitAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackAsync(context.CancellationToken);
                _logger.LogError(ex, $"Error processing {nameof(ProcessPaymentConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
