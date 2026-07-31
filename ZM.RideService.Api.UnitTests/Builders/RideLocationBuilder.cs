using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.UnitTests.Builders
{
    internal static class RideLocationBuilder
    {
        public static RideLocation Build(double latitude = 1.0, double longitude = 2.0, string address = "Address")
        {
            return new RideLocation(latitude, longitude, address);
        }
    }
}
