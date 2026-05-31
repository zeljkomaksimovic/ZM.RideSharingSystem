#nullable disable
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Domain.Entities
{
    public class Ride
    {
        private Ride()
        {
        }

        private Ride(Guid rideId, Guid riderId, RideLocation pickupLocation, RideLocation destinationLocation, RideStatus status, DateTime createdAtUtc)
        {
            RiderId = rideId;
            RiderId = riderId;
            PickupLocation = pickupLocation;
            DestinationLocation = destinationLocation;
            Status = status;
            CreatedAtUtc = createdAtUtc;
        }

        public Guid Id { get; private set; }
        public Guid RiderId { get; private set; }
        public Guid? DriverId { get; private set; }
        public RideLocation PickupLocation { get; private set; }
        public RideLocation DestinationLocation { get; private set; }
        public RideStatus Status { get; private set; }
        public decimal? EstimatedFare { get; private set; }
        public decimal? ActualFare { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? AssignedAtUtc { get; private set; }
        public DateTime? StartedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        public DateTime? CancelledAtUtc { get; private set; }

        public static Ride Create(Guid riderId, RideLocation pickupLocation, RideLocation destinationLocation, DateTime createdAtUtc)
        {
            return new Ride
                (Guid.NewGuid(),
                riderId,
                pickupLocation,
                destinationLocation,
                RideStatus.Requested,
                createdAtUtc);
        }
    }
}
