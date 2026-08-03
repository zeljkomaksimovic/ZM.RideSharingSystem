using ZM.DriverService.Api.Domain.ValueObjects;

namespace ZM.DriverService.Api.UnitTests.Builders
{
    internal static class DriverLocationBuilder
    {
        public static DriverLocation Build(
            double latitude = 1.0,
            double longitude = 2.0)
            => new(latitude, longitude);
    }
}