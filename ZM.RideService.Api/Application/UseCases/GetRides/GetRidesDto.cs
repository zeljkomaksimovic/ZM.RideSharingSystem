using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Application.UseCases.GetRides
{
    public record GetRidesDto(
        Guid Id,
        Guid RiderId,
        Guid? DriverId,
        RideLocation PickupLocation,
        RideLocation DestinationLocation,
        RideStatus Status,
        decimal? EstimatedFare,
        decimal? ActualFare,
        DateTime CreatedAtUtc,
        DateTime? AssignedAtUtc,
        DateTime? StartedAtUtc,
        DateTime? CompletedAtUtc,
        DateTime? CancelledAtUtc);
}
