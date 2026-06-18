using ZM.DriverService.Api.Domain.Primitives;

namespace ZM.DriverService.Api.Domain.Events
{
    public record RideAssignedDomainEvent(Guid RideId, Guid DriverId) : IDomainEvent;
}
