using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ValueObjects;
using ZM.RideService.Api.UnitTests.Builders;

internal static class RideBuilder
{
    public static Ride BuildRequested(
        Guid? id = null,
        RiderInfo? rider = null,
        RideLocation? pickup = null,
        RideLocation? destination = null,
        decimal? estimatedFare = 10m,
        DateTime? createdAt = null)
    {
        return Ride.Rehydrate(
            id ?? Guid.NewGuid(),
            rider ?? RiderInfoBuilder.Build(),
            null,
            pickup ?? RideLocationBuilder.Build(),
            destination ?? RideLocationBuilder.Build(3.0, 4.0, "Destination"),
            RideStatus.Requested,
            estimatedFare,
            null,
            createdAt ?? DateTime.UtcNow,
            null,
            null,
            null,
            null);
    }

    public static Ride BuildAssigned(
        Guid? id = null,
        RiderInfo? rider = null,
        RideLocation? pickup = null,
        RideLocation? destination = null,
        decimal? estimatedFare = 10m,
        Guid? driverId = null,
        DateTime? createdAt = null,
        DateTime? assignedAt = null)
    {
        return Ride.Rehydrate(
            id ?? Guid.NewGuid(),
            rider ?? RiderInfoBuilder.Build(),
            driverId ?? Guid.NewGuid(),
            pickup ?? RideLocationBuilder.Build(),
            destination ?? RideLocationBuilder.Build(3.0, 4.0, "Destination"),
            RideStatus.DriverAssigned,
            estimatedFare,
            null,
            createdAt ?? DateTime.UtcNow.AddMinutes(-10),
            assignedAt ?? DateTime.UtcNow.AddMinutes(-5),
            null,
            null,
            null);
    }

    public static Ride BuildInProgress(
        Guid? id = null,
        RiderInfo? rider = null,
        RideLocation? pickup = null,
        RideLocation? destination = null,
        decimal? estimatedFare = 10m,
        Guid? driverId = null,
        DateTime? createdAt = null,
        DateTime? assignedAt = null,
        DateTime? startedAt = null)
    {
        return Ride.Rehydrate(
            id ?? Guid.NewGuid(),
            rider ?? RiderInfoBuilder.Build(),
            driverId ?? Guid.NewGuid(),
            pickup ?? RideLocationBuilder.Build(),
            destination ?? RideLocationBuilder.Build(3.0, 4.0, "Destination"),
            RideStatus.InProgress,
            estimatedFare,
            null,
            createdAt ?? DateTime.UtcNow.AddMinutes(-10),
            assignedAt ?? DateTime.UtcNow.AddMinutes(-5),
            startedAt ?? DateTime.UtcNow.AddMinutes(-2),
            null,
            null);
    }

    public static Ride BuildCompleted(
    Guid? id = null,
    RiderInfo? rider = null,
    RideLocation? pickup = null,
    RideLocation? destination = null,
    decimal? estimatedFare = 10m,
    decimal? actualFare = 15m,
    Guid? driverId = null)
    {
        return Ride.Rehydrate(
            id ?? Guid.NewGuid(),
            rider ?? RiderInfoBuilder.Build(),
            driverId ?? Guid.NewGuid(),
            pickup ?? RideLocationBuilder.Build(),
            destination ?? RideLocationBuilder.Build(3.0, 4.0, "Destination"),
            RideStatus.Completed,
            estimatedFare,
            actualFare,
            DateTime.UtcNow.AddMinutes(-30),
            DateTime.UtcNow.AddMinutes(-25),
            DateTime.UtcNow.AddMinutes(-20),
            DateTime.UtcNow.AddMinutes(-5),
            null);
    }
}