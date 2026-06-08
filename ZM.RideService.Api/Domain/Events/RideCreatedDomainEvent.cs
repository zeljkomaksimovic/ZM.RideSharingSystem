using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Domain.Events
{
    public record RideCreatedDomainEvent(Guid RideId, Guid RiderId) : IDomainEvent;
}
