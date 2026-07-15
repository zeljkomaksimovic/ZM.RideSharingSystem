using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Domain.Events
{
    public record RideCompletedDomainEvent(Ride Ride) : IDomainEvent;
}
