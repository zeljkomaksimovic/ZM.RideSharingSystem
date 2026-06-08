#nullable disable
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ErrorMessages;
using ZM.RideService.Api.Domain.Events;
using ZM.RideService.Api.Domain.OperationResult;
using ZM.RideService.Api.Domain.Primitives;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Domain.Entities
{
    public class Ride : AggregateRoot
    {
        private Ride()
        {
        }

        private Ride(Guid rideId, Guid riderId, RideLocation pickupLocation, RideLocation destinationLocation, RideStatus status, DateTime createdAtUtc)
        {
            Id = rideId;
            RiderId = riderId;
            PickupLocation = pickupLocation;
            DestinationLocation = destinationLocation;
            Status = status;
            CreatedAtUtc = createdAtUtc;
        }

        private Ride(
            Guid id,
            Guid riderId,
            Guid? driverId,
            RideLocation pickupLocation,
            RideLocation destinationLocation,
            RideStatus status,
            decimal? estimatedFare,
            decimal? actualFare,
            DateTime createdAtUtc,
            DateTime? assignedAtUtc,
            DateTime? startedAtUtc,
            DateTime? completedAtUtc,
            DateTime? cancelledAtUtc)
        {
            Id = id;
            RiderId = riderId;
            DriverId = driverId;
            PickupLocation = pickupLocation;
            DestinationLocation = destinationLocation;
            Status = status;
            EstimatedFare = estimatedFare;
            ActualFare = actualFare;
            CreatedAtUtc = createdAtUtc;
            AssignedAtUtc = assignedAtUtc;
            StartedAtUtc = startedAtUtc;
            CompletedAtUtc = completedAtUtc;
            CancelledAtUtc = cancelledAtUtc;
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

        public static Ride Create(Guid rideId, Guid riderId, RideLocation pickupLocation, RideLocation destinationLocation, DateTime createdAtUtc)
        {
            var ride = new Ride(
                rideId,
                riderId,
                pickupLocation,
                destinationLocation,
                RideStatus.Requested,
                createdAtUtc);

            ride.Raise(new RideCreatedDomainEvent(rideId, riderId));

            return ride;
        }

        public static Result AssignDriver(Ride ride, Guid driverId, DateTime assignedAtUtc)
        {
            if (ride.Status != RideStatus.Requested)
            {
                return Result.Failure(Errors.Ride.DriverCouldNotBeAssigned());
            }

            ride.DriverId = driverId;
            ride.Status = RideStatus.DriverAssigned;
            ride.AssignedAtUtc = assignedAtUtc;

            return Result.Success();
        }

        public static Result StartRide(Ride ride, DateTime startedAtUtc)
        {
            if (ride.Status != RideStatus.DriverAssigned)
            {
                return Result.Failure(Errors.Ride.DriverIsNotAssigned());
            }

            ride.Status = RideStatus.InProgress;
            ride.StartedAtUtc = startedAtUtc;

            return Result.Success();
        }

        public static Result CompleteRide(Ride ride, decimal actualFare, DateTime completedAtUtc)
        {
            if (ride.Status != RideStatus.InProgress)
            {
                return Result.Failure(Errors.Ride.RideIsNotInProgress());
            }

            ride.Status = RideStatus.Completed;
            ride.ActualFare = actualFare;
            ride.CompletedAtUtc = completedAtUtc;

            return Result.Success();
        }

        public static Result CancelRide(Ride ride, DateTime cancelledAtUtc)
        {
            if (ride.Status == RideStatus.Completed)
            {
                return Result.Failure(Errors.Ride.RideCannotBeCancelled());
            }

            ride.Status = RideStatus.Cancelled;
            ride.CancelledAtUtc = cancelledAtUtc;

            return Result.Success();
        }

        public static Ride Rehydrate(
            Guid id,
            Guid riderId,
            Guid? driverId,
            RideLocation pickupLocation,
            RideLocation destinationLocation,
            RideStatus status,
            decimal? estimatedFare,
            decimal? actualFare,
            DateTime createdAtUtc,
            DateTime? assignedAtUtc,
            DateTime? startedAtUtc,
            DateTime? completedAtUtc,
            DateTime? cancelledAtUtc)
        {
            return new Ride(
                id,
                riderId,
                driverId,
                pickupLocation,
                destinationLocation,
                status,
                estimatedFare,
                actualFare,
                createdAtUtc,
                assignedAtUtc,
                startedAtUtc,
                completedAtUtc,
                cancelledAtUtc);           
        }
    }
}
