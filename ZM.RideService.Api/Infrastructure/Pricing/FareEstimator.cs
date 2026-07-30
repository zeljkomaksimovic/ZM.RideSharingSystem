using ZM.RideService.Api.Application.Pricing;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Infrastructure.Pricing
{
    public class FareEstimator : IFareEstimator
    {
        private const decimal BaseFare = 2.50m;
        private const decimal PricePerKm = 1.20m;

        public Task<decimal> EstimateAsync(RideLocation pickup, RideLocation destination, CancellationToken cancellationToken = default)
        {
            var distance = CalculateDistance(
                pickup.Latitude,
                pickup.Longitude,
                destination.Latitude,
                destination.Longitude);

            var estimatedFare = BaseFare + (decimal)distance * PricePerKm;

            return Task.FromResult(decimal.Round(estimatedFare, 2, MidpointRounding.AwayFromZero));
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
