using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MediatR;
using Quartz;
using Polly;
using Polly.Retry;
using ZM.RideService.Api.Domain.Primitives;
using ZM.RideService.Api.Persistence;

namespace ZM.RideService.Api.Infrastructure.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class ProcessOutboxMessagesJob : IJob
    {
        private readonly RideDbContext _dbContext;
        private readonly ILogger<ProcessOutboxMessagesJob> _logger;
        private readonly IPublisher _publisher;

        public ProcessOutboxMessagesJob(
            RideDbContext dbContext,
            ILogger<ProcessOutboxMessagesJob> logger,
            IPublisher publisher)
        {
            _logger = logger;
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var messages = await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .Take(20)
                .ToListAsync(context.CancellationToken);

            foreach (var message in messages)
            {

                var domainEvent = JsonConvert
               .DeserializeObject<IDomainEvent>(
                   message.Content,
                   new JsonSerializerSettings
                   {
                       TypeNameHandling = TypeNameHandling.All,
                       ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                   });

                if (domainEvent is null)
                {
                    message.Error = "Deserialization returned null.";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var pipeline = new ResiliencePipelineBuilder()
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Constant,
                        Delay = TimeSpan.FromMilliseconds(50),
                        ShouldHandle = new PredicateBuilder()
                            .Handle<Exception>(),
                        OnRetry = async retryArguments =>
                        {
                            _logger.LogError("Error occured while attempting to publish message.");
                        }
                    })
                    .Build();
                try
                {
                    await pipeline.ExecuteAsync(async token =>
                        await _publisher.Publish(domainEvent, token),
                        context.CancellationToken);

                    message.ProcessedOnUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.Error = $"Error occured: {ex.ToString()}";
                }

            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
