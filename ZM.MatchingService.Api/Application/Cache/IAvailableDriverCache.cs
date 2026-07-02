using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Application.Cache
{
    public interface IAvailableDriverCache
    {
        Task AddOrUpdateDriverAsync(AvailableDriver driver, CancellationToken cancellationToken = default);
        Task RemoveDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
        Task<AvailableDriver?> GetNearestDriverAsync(RideLocation pickupLocation, CancellationToken cancellationToken = default);
    }
}
