using ZM.MatchingService.Api.Domain.Models;

namespace ZM.MatchingService.Api.UnitTests.Builders
{
    internal static class AvailableDriverBuilder
    {
        public static AvailableDriver Build(
            Guid? driverId = null,
            double latitude = 44.7866,
            double longitude = 20.4489,
            DateTime? lastLocationUpdateAtUtc = null)
        {
            return new AvailableDriver(
                driverId ?? Guid.NewGuid(),
                latitude,
                longitude,
                lastLocationUpdateAtUtc ?? DateTime.UtcNow);
        }
    }
}