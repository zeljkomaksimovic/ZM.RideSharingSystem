using ZM.RideService.Api.Domain.Entities;

namespace ZM.RideService.Api.Application.Repository
{
    public interface IRideRepository
    {
        Task CreateRideAsync(Ride ride, CancellationToken cancellationToken = default);
    }
}
