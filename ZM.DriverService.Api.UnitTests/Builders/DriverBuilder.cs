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
                DriverStatus.Offline,
                createdAt ?? DateTime.UtcNow,
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
                createdAt ?? DateTime.UtcNow.AddMinutes(-10),
                locationUpdatedAt ?? DateTime.UtcNow.AddMinutes(-5),
                statusChangedAt ?? DateTime.UtcNow.AddMinutes(-5));
        }

        public static Driver BuildAssigned(
            Guid? id = null,
            Guid? rideId = null,
            DriverLocation? location = null)
        {
            var driver = Driver.Rehydrate(
                id ?? Guid.NewGuid(),
                "First",
                "Last",
                "email@example.com",
                "123",
                location ?? new DriverLocation(1.0, 2.0),
                DriverStatus.Assigned,
                DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(-2));

            driver.GetType()
                .GetProperty(nameof(Driver.CurrentRideId))!
                .SetValue(driver, rideId ?? Guid.NewGuid());

            return driver;
        }

        public static Driver BuildInRide(
            Guid? id = null,
            Guid? rideId = null,
            DriverLocation? location = null)
        {
            var driver = Driver.Rehydrate(
                id ?? Guid.NewGuid(),
                "First",
                "Last",
                "email@example.com",
                "123",
                location ?? new DriverLocation(1.0, 2.0),
                DriverStatus.InRide,
                DateTime.UtcNow.AddMinutes(-20),
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(-2));

            driver.GetType()
                .GetProperty(nameof(Driver.CurrentRideId))!
                .SetValue(driver, rideId ?? Guid.NewGuid());

            return driver;
        }
    }
}