using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Application.Cache
{
    public interface IAvailableDriverCache
    {
        Task AddAvailableDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
        Task RemoveDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
        Task UpdateDriverLocationAsync(Guid driverId, GeoLocation location, DateTime updatedAtUtc, CancellationToken cancellationToken = default);
        Task<AvailableDriver?> GetNearestDriverAsync(GeoLocation pickupLocation, CancellationToken cancellationToken = default);
    }
}
