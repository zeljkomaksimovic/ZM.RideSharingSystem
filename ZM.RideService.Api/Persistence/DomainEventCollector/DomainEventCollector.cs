using ZM.RideService.Api.Application.DomainEventCollector;
using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Persistence.DomainEventCollector
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

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
