using ZM.RideService.Api.Domain.Primitives;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Domain.Events
{
    public sealed record RideCreatedDomainEvent(Guid RideId, double Latitude, double Longitude, RiderInfo Rider) : IDomainEvent;
}