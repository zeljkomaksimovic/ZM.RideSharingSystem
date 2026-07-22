using ZM.RideService.Api.Domain.Primitives;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Domain.Events
{
    public record RideCompletedDomainEvent(Guid RideId, RiderInfo Rider) : IDomainEvent;
}
