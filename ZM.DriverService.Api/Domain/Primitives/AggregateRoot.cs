namespace ZM.DriverService.Api.Domain.Primitives
{
    public class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.ToList();

        public void ClearDomainEvents() => _domainEvents.Clear();

        protected void Raise(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
