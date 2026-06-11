using ZM.RideService.Api.Domain.Primitives;

namespace ZM.RideService.Api.Domain.Events
{
    public record DriverAssignedDomainEvent(Guid RideId, Guid DriverId) : IDomainEvent;
}
