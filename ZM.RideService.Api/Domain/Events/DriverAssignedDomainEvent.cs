using ZM.RideService.Api.Domain.Primitives;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Domain.Events
{
    public record DriverAssignedDomainEvent(Guid RideId, Guid DriverId) : IDomainEvent;
}
