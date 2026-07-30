using ZM.RideService.Api.Application.Pricing;
using ZM.RideService.Api.Domain.Entities;

namespace ZM.RideService.Api.Infrastructure.Pricing
{
    public class FareCalculator : IFareCalculator
    {
        private const decimal BaseFare = 2.50m;
        private const decimal PricePerKilometer = 1.20m;
        private const decimal PricePerMinute = 0.30m;

        public Task<decimal> CalculateAsync(Ride ride, DateTime completedAtUtc, CancellationToken cancellationToken = default)
        {
            var distance = CalculateDistance(
                ride.PickupLocation.Latitude,
                ride.PickupLocation.Longitude,
                ride.DestinationLocation.Latitude,
                ride.DestinationLocation.Longitude);

            var duration = completedAtUtc - ride.StartedAtUtc!.Value;

            var fare =
                BaseFare +
                (decimal)distance * PricePerKilometer +
                (decimal)duration.TotalMinutes * PricePerMinute;

            return Task.FromResult(decimal.Round(fare, 2, MidpointRounding.AwayFromZero));
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
