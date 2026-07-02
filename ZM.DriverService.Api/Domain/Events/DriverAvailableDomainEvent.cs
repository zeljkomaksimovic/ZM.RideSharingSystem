using ZM.DriverService.Api.Domain.Primitives;

namespace ZM.DriverService.Api.Domain.Events
{
    public record DriverAvailableDomainEvent(Guid DriverId, double Latitude, double Longitude) : IDomainEvent;
}
