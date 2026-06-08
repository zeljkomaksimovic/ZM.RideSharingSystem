using ZM.RideService.Api.Domain.Entities;

namespace ZM.RideService.Api.Application.Repository
{
    public interface IRideRepository
    {
        Task<Ride?> GetRideByIdAsync(Guid rideId, CancellationToken cancellationToken = default);
        Task CreateRideAsync(Ride ride, CancellationToken cancellationToken = default);
        Task AssignDriverAsync(Ride ride, CancellationToken cancellationToken = default);
        Task StartRideAsync(Ride ride, CancellationToken cancellationToken = default);
        Task CompleteRideAsync(Ride ride, CancellationToken cancellationToken = default);
        Task CancelRideAsync(Ride ride, CancellationToken cancellationToken = default);
    }
}
