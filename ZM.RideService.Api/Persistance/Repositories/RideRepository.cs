using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Domain.Entities;

namespace ZM.RideService.Api.Persistance.Repositories
{
    public class RideRepository : IRideRepository
    {
        private readonly RideDbContext _dbContext;

        public RideRepository(RideDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateRideAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            var dbRide = new Models.Ride
            {
                Id = ride.Id,
                PickupLatitude = ride.PickupLocation.Latitude,
                PickupLongitude = ride.PickupLocation.Longitude,
                PickupAddress = ride.PickupLocation.Address,
                DestinationLatitude = ride.DestinationLocation.Latitude,
                DestinationLongitude = ride.DestinationLocation.Longitude,
                DestinationAddress = ride.DestinationLocation.Address,
                CreatedAtUtc = ride.CreatedAtUtc,
                Status = (int)ride.Status,
            };

            await _dbContext.Rides.AddAsync(dbRide, cancellationToken);
        }
    }
}
