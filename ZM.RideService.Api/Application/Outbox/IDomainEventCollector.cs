using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Application.Outbox
{
    public interface IDomainEventCollector
    {
        void AddEvent(IDomainEvent domainEvent);
        void AddEvents(IEnumerable<IDomainEvent> domainEvents);
        IReadOnlyCollection<IDomainEvent> GetDomainEvents();
        void ClearEvents();
    }
}
