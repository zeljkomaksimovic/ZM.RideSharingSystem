using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ValueObjects;

namespace ZM.DriverService.Api.UnitTests.Builders
{
    internal static class DriverBuilder
    {
        public static Driver BuildOffline(
            Guid? id = null,
            string firstName = "First",
            string lastName = "Last",
            string email = "email@example.com",
            string phone = "123",
            DateTime? createdAt = null)
        {
            return Driver.Rehydrate(
                id ?? Guid.NewGuid(),
                firstName,
                lastName,
                email,
                phone,
                currentLocation: null,
                status: DriverStatus.Offline,
                currentRideId: null,
                createdAtUtc: createdAt ?? DateTime.UtcNow,
                lastLocationUpdateAtUtc: null,
                lastStatusChangeAtUtc: null);
        }

        public static Driver BuildAvailable(
            Guid? id = null,
            DriverLocation? location = null,
            DateTime? createdAt = null,
            DateTime? statusChangedAt = null,
            DateTime? locationUpdatedAt = null)
        {
            return Driver.Rehydrate(
                id ?? Guid.NewGuid(),
                "First",
                "Last",
                "email@example.com",
                "123",
                location ?? new DriverLocation(1.0, 2.0),
                DriverStatus.Available,
                currentRideId: null,
                createdAtUtc: createdAt ?? DateTime.UtcNow.AddMinutes(-10),
                lastLocationUpdateAtUtc: locationUpdatedAt ?? DateTime.UtcNow.AddMinutes(-5),
                lastStatusChangeAtUtc: statusChangedAt ?? DateTime.UtcNow.AddMinutes(-5));
        }

        public static Driver BuildAssigned(
            Guid? id = null,
            Guid? rideId = null,
            DriverLocation? location = null,
            DateTime? createdAt = null,
            DateTime? statusChangedAt = null,
            DateTime? locationUpdatedAt = null)
        {
            return Driver.Rehydrate(
                id ?? Guid.NewGuid(),
                "First",
                "Last",
                "email@example.com",
                "123",
                location ?? new DriverLocation(1.0, 2.0),
                DriverStatus.Assigned,
                rideId ?? Guid.NewGuid(),
                createdAtUtc: createdAt ?? DateTime.UtcNow.AddMinutes(-20),
                lastLocationUpdateAtUtc: locationUpdatedAt ?? DateTime.UtcNow.AddMinutes(-2),
                lastStatusChangeAtUtc: statusChangedAt ?? DateTime.UtcNow.AddMinutes(-2));
        }

        public static Driver BuildInRide(
            Guid? id = null,
            Guid? rideId = null,
            DriverLocation? location = null,
            DateTime? createdAt = null,
            DateTime? statusChangedAt = null,
            DateTime? locationUpdatedAt = null)
        {
            return Driver.Rehydrate(
                id ?? Guid.NewGuid(),
                "First",
                "Last",
                "email@example.com",
                "123",
                location ?? new DriverLocation(1.0, 2.0),
                DriverStatus.InRide,
                rideId ?? Guid.NewGuid(),
                createdAtUtc: createdAt ?? DateTime.UtcNow.AddMinutes(-30),
                lastLocationUpdateAtUtc: locationUpdatedAt ?? DateTime.UtcNow.AddMinutes(-2),
                lastStatusChangeAtUtc: statusChangedAt ?? DateTime.UtcNow.AddMinutes(-1));
        }
    }
}