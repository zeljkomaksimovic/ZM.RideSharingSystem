using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Domain.Events
{
    public record RideCompletedDomainEvent(Guid RideId) : IDomainEvent;
}
