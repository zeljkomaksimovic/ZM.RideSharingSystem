using System.Globalization;
using StackExchange.Redis;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Infrastructure.Caches
{
    public class RedisDriverMatchingCache : IAvailableDriverCache
    {
        private const string AvailableDriversKey = "available-drivers";
        private const string DriverKeyPrefix = "driver:";
        private const double SearchRadiusInKilometers = 10;

        private readonly IDatabase _database;

        public RedisDriverMatchingCache(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task AddOrUpdateDriverAsync(AvailableDriver driver, CancellationToken cancellationToken = default)
        {
            await _database.GeoAddAsync(
                AvailableDriversKey,
                driver.Longitude,
                driver.Latitude,
                driver.DriverId.ToString());

            var metadataKey = GetMetadataKey(driver.DriverId);

            await _database.HashSetAsync(metadataKey, new[]
            {
                new HashEntry(
                    nameof(AvailableDriver.LastLocationUpdateAtUtc),
                    driver.LastLocationUpdateAtUtc.ToString("O"))
            });
        }

        public async Task RemoveDriverAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            await _database.GeoRemoveAsync(AvailableDriversKey, driverId.ToString());
            await _database.KeyDeleteAsync(GetMetadataKey(driverId));
        }

        public async Task<AvailableDriver?> GetNearestDriverAsync(RideLocation pickupLocation, CancellationToken cancellationToken = default)
        {
            var searchResult = await _database.GeoRadiusAsync(
                AvailableDriversKey,
                pickupLocation.Longitude,
                pickupLocation.Latitude,
                SearchRadiusInKilometers,
                GeoUnit.Kilometers,
                count: 1,
                order: Order.Ascending);

            if (searchResult.Length == 0)
            {
                return null;
            }

            var nearestDriver = searchResult[0];

            if (!nearestDriver.Position.HasValue)
            {
                return null;
            }

            var driverId = Guid.Parse(nearestDriver.Member.ToString());

            var metadata = await _database.HashGetAllAsync(
                GetMetadataKey(driverId));

            var values = metadata.ToDictionary(
                entry => entry.Name.ToString(),
                entry => entry.Value.ToString());

            if (!values.TryGetValue(
                nameof(AvailableDriver.LastLocationUpdateAtUtc),
                out var lastLocationUpdate))
            {
                return null;
            }

            return new AvailableDriver(
                driverId,
                nearestDriver.Position.Value.Latitude,
                nearestDriver.Position.Value.Longitude,
                DateTime.Parse(
                    lastLocationUpdate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind));
        }

        private static string GetMetadataKey(Guid driverId)
        {
            return $"{DriverKeyPrefix}{driverId}";
        }
    }
}