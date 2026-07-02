using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Domain.Services
{
    public class DriverMatchingService : IDriverMatchingService
    {
        public AvailableDriver? FindBestDriver( RideLocation pickupLocation, IReadOnlyCollection<AvailableDriver> availableDrivers)
        {
            return availableDrivers
                .OrderBy(driver => CalculateDistance(
                    pickupLocation.Latitude,
                    pickupLocation.Longitude,
                    driver.Latitude,
                    driver.Longitude))
                .FirstOrDefault();
        }

        private static double CalculateDistance(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            const double earthRadiusKm = 6371;

            var dLat = DegreesToRadians(latitude2 - latitude1);
            var dLon = DegreesToRadians(longitude2 - longitude1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(latitude1)) *
                Math.Cos(DegreesToRadians(latitude2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}