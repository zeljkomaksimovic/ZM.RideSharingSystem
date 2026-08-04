using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.Data;
using ZM.RideService.Api.Application.DomainEventCollector;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Persistence.Outbox;

namespace ZM.RideService.Api.Persistence.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RideDbContext _dbContext;
        private readonly IDomainEventCollector _domainEventCollector;

        public UnitOfWork(RideDbContext dbContext, IDomainEventCollector domainEventCollector)
        {
            _dbContext = dbContext;
            _domainEventCollector = domainEventCollector;
        }

        public IDbTransaction BeginTransaction()
        {
            var transaction = _dbContext.Database.BeginTransaction();
            return transaction.GetDbTransaction();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDomainEventsToOutboxMessages();
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDomainEventsToOutboxMessages()
        {
            var outboxMessages = _domainEventCollector
                .GetDomainEvents()
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    Content = JsonConvert.SerializeObject(
                        domainEvent,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        })
                })
                .ToList();
            
            _dbContext.OutboxMessages.AddRange(outboxMessages);

            _domainEventCollector.ClearDomainEvents();
        }
    }
}
