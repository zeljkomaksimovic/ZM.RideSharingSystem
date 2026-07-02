using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Domain.Services
{
    public interface IDriverMatchingService
    {
        AvailableDriver? FindBestDriver(RideLocation pickupLocation, IReadOnlyCollection<AvailableDriver> availableDrivers);
    }
}