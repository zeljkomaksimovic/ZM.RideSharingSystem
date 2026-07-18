using ZM.RideService.Api.Application.Outbox;
using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Persistence.Outbox
{
    public class DomainEventCollector : IDomainEventCollector
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public void AddEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void AddEvents(IEnumerable<IDomainEvent> domainEvents)
        {
            _domainEvents.AddRange(domainEvents);
        }

        public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        public void ClearEvents()
        {
            _domainEvents.Clear();
        }
    }
}
