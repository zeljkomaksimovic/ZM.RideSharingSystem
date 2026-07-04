using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZM.PaymentService.Api.Persistence;
using ZM.RideSharingSystem.Contracts.Events;
using ZM.PaymentService.Api.Application.UseCases.ProcessPayment;

namespace ZM.PaymentService.Api.Infrastructure.Consumers
{
    public class ProcessPaymentCommandConsumer : IConsumer<RideCompletedEvent>
    {
        private readonly ILogger<ProcessPaymentCommandConsumer> _logger;
        private readonly ISender _sender;
        private readonly PaymentDbContext _dbContext;

        public ProcessPaymentCommandConsumer(ILogger<ProcessPaymentCommandConsumer> logger, ISender sender, PaymentDbContext dbContext)
        {
            _logger = logger;
            _sender = sender;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<RideCompletedEvent> context)
        {
            if (await _dbContext.ProcessedMessages.AnyAsync(p =>
                    p.MessageId == context.MessageId &&
                    p.ConsumerName == nameof(ProcessPaymentCommandConsumer),
                context.CancellationToken))
            {
                return;
            }

            try
            {
                await _dbContext.ProcessedMessages.AddAsync(new Persistence.Idempotence.ProcessedMessage
                {
                    MessageId = context.MessageId,
                    ConsumerName = nameof(ProcessPaymentCommandConsumer),
                    CreatedAtUtc = DateTime.UtcNow
                }, context.CancellationToken);

                // For now amount is unknown; set to 0 and let handler or downstream logic determine actual amount
                await _sender.Send(new ProcessPaymentCommand(context.Message.RideId, 0m), context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {nameof(ProcessPaymentCommandConsumer)} with MessageId: {context.MessageId}");
            }
        }
    }
}
