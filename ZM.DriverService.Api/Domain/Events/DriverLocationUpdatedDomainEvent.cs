using ZM.DriverService.Api.Domain.Primitives;

namespace ZM.DriverService.Api.Domain.Events
{
    public record DriverLocationUpdatedDomainEvent(Guid DriverId, double Latitude, double Longitude) : IDomainEvent;
}
