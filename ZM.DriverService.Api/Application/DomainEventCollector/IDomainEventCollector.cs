using ZM.DriverService.Api.Domain.Primitives;

namespace ZM.DriverService.Api.Application.DomainEventCollector
{
    public interface IDomainEventCollector
    {
        void AddEvent(IDomainEvent domainEvent);
        void AddEvents(IEnumerable<IDomainEvent> domainEvents);
        IReadOnlyCollection<IDomainEvent> GetDomainEvents();
        void ClearDomainEvents();
    }
}
