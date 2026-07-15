using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Domain.Events
{
    public record DriverAssignedDomainEvent(Ride Ride) : IDomainEvent;
}
