using Microsoft.EntityFrameworkCore;
using ZM.RideService.Api.Application.Outbox;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Persistence.Repositories
{
    public class RideRepository : IRideRepository
    {
        private readonly RideDbContext _dbContext;
        private readonly IDomainEventCollector _domainEventCollector;

        public RideRepository(RideDbContext dbContext, IDomainEventCollector domainEventCollector)
        {
            _dbContext = dbContext;
            _domainEventCollector = domainEventCollector;
        }

        public async Task AssignDriverAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            var dbRide = await _dbContext.Rides
                .Where(r => r.Id == ride.Id)
                .SingleOrDefaultAsync(cancellationToken);
            
            dbRide!.DriverId = ride.DriverId;
            dbRide.Status = ride.Status;
            dbRide.AssignedAtUtc = ride.AssignedAtUtc;

            _domainEventCollector.AddEvents(ride.DomainEvents);

            ride.ClearDomainEvents();
        }

        public async Task CancelRideAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            var dbRide = await _dbContext.Rides
                .Where(r => r.Id == ride.Id)
                .SingleOrDefaultAsync(cancellationToken);

            dbRide!.Status = ride.Status;
            dbRide.CancelledAtUtc = ride.CancelledAtUtc;

            _domainEventCollector.AddEvents(ride.DomainEvents);

            ride.ClearDomainEvents();
        }

        public async Task CompleteRideAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            var dbRide = await _dbContext.Rides
                .Where(r => r.Id == ride.Id)
                .SingleOrDefaultAsync(cancellationToken);

            dbRide!.Status = ride.Status;
            dbRide.ActualFare = ride.ActualFare;
            dbRide.CompletedAtUtc = ride.CompletedAtUtc;

            _domainEventCollector.AddEvents(ride.DomainEvents);

            ride.ClearDomainEvents();
        }

        public async Task CreateRideAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            var dbRide = new Models.Ride
            {
                Id = ride.Id,
                RiderId = ride.Rider.Id,
                RiderFirstName = ride.Rider.FirstName,
                RiderLastName = ride.Rider.LastName,
                RiderEmail = ride.Rider.Email,
                DriverId = ride.DriverId,
                PickupLatitude = ride.PickupLocation.Latitude,
                PickupLongitude = ride.PickupLocation.Longitude,
                PickupAddress = ride.PickupLocation.Address,
                DestinationLatitude = ride.DestinationLocation.Latitude,
                DestinationLongitude = ride.DestinationLocation.Longitude,
                DestinationAddress = ride.DestinationLocation.Address,
                Status = ride.Status,
                EstimatedFare = ride.EstimatedFare,
                ActualFare = ride.ActualFare,
                CreatedAtUtc = ride.CreatedAtUtc,
                AssignedAtUtc = ride.AssignedAtUtc,
                StartedAtUtc = ride.StartedAtUtc,
                CompletedAtUtc = ride.CompletedAtUtc,
                CancelledAtUtc = ride.CancelledAtUtc
            };

            await _dbContext.Rides.AddAsync(dbRide, cancellationToken);

            _domainEventCollector.AddEvents(ride.DomainEvents);

            ride.ClearDomainEvents();
        }

        public async Task<Ride?> GetRideByIdAsync(Guid rideId, CancellationToken cancellationToken = default)
        {
            var ride =  await _dbContext.Rides
                .Where(r => r.Id == rideId)
                .Select(r => Ride.Rehydrate(
                    r.Id,
                    new RiderInfo(
                        r.RiderId,
                        r.RiderFirstName,
                        r.RiderLastName,
                        r.RiderEmail),
                    r.DriverId,
                    new RideLocation(
                        r.PickupLatitude,
                        r.PickupLongitude,
                        r.PickupAddress),
                    new RideLocation(
                        r.DestinationLatitude,
                        r.DestinationLongitude,
                        r.DestinationAddress),
                    r.Status,
                    r.EstimatedFare,
                    r.ActualFare,
                    r.CreatedAtUtc,
                    r.AssignedAtUtc,
                    r.StartedAtUtc,
                    r.CompletedAtUtc,
                    r.CancelledAtUtc))
                .FirstOrDefaultAsync(cancellationToken);

            return ride;
        }

        public async Task StartRideAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            var dbRide = await _dbContext.Rides
                .Where(r => r.Id == ride.Id)
                .SingleOrDefaultAsync(cancellationToken);

            dbRide!.Status = ride.Status;
            dbRide.StartedAtUtc = ride.StartedAtUtc;

            _domainEventCollector.AddEvents(ride.DomainEvents);

            ride.ClearDomainEvents();
        }
    }
}
