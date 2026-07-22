using StackExchange.Redis;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Infrastructure.Caches
{
    public class RedisDriverMatchingCache : IAvailableDriverCache
    {
        private const string AvailableDriversSetKey = "drivers:available";
        private const string DriversGeoKey = "drivers:geo";
        private const string DriverMetadataPrefix = "driver:";

        private readonly IDatabase _database;

        public RedisDriverMatchingCache(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task AddAvailableDriverAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            var a = await _database.SetAddAsync(
                AvailableDriversSetKey,
                driverId.ToString());
        }

        public async Task RemoveDriverAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            var member = driverId.ToString();

            await _database.SetRemoveAsync(AvailableDriversSetKey, member);

            await _database.GeoRemoveAsync(DriversGeoKey, member);

            await _database.KeyDeleteAsync(GetMetadataKey(driverId));
        }

        public async Task UpdateDriverLocationAsync(Guid driverId, GeoLocation location, DateTime updatedAtUtc, CancellationToken cancellationToken = default)
        {
            var member = driverId.ToString();

            if (!await _database.SetContainsAsync(
                    AvailableDriversSetKey,
                    member))
            {
                return;
            }

            await _database.GeoAddAsync(
                DriversGeoKey,
                location.Longitude,
                location.Latitude,
                member);

            await _database.HashSetAsync(
                GetMetadataKey(driverId),
                new[]
                {
                    new HashEntry(
                        nameof(AvailableDriver.LastLocationUpdateAtUtc),
                        updatedAtUtc.ToString("O"))
                });
        }

        public async Task<AvailableDriver?> GetNearestDriverAsync(
            GeoLocation pickupLocation,
            CancellationToken cancellationToken = default)
        {
            var drivers = await _database.GeoRadiusAsync(
                DriversGeoKey,
                pickupLocation.Longitude,
                pickupLocation.Latitude,
                10,
                GeoUnit.Kilometers,
                order: Order.Ascending);

            foreach (var driver in drivers)
            {
                var member = driver.Member.ToString();

                if (!await _database.SetContainsAsync(
                        AvailableDriversSetKey,
                        member))
                {
                    continue;
                }

                var metadata = await _database.HashGetAllAsync(GetMetadataKey(Guid.Parse(member)));

                var values = metadata.ToDictionary(
                    x => x.Name.ToString(),
                    x => x.Value.ToString());

                return new AvailableDriver(
                    Guid.Parse(member),
                    driver.Position!.Value.Latitude,
                    driver.Position!.Value.Longitude,
                    DateTime.Parse(values[nameof(AvailableDriver.LastLocationUpdateAtUtc)]));
            }

            return null;
        }

        private static string GetMetadataKey(Guid driverId) => $"{DriverMetadataPrefix}{driverId}";
    }
}