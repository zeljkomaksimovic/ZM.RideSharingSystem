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

        public Result AssignDriver(Guid driverId, DateTime assignedAtUtc)
        {
            if (Status != RideStatus.Requested)
            {
                return Result.Failure(Errors.Ride.DriverCouldNotBeAssigned());
            }

            DriverId = driverId;
            Status = RideStatus.DriverAssigned;
            AssignedAtUtc = assignedAtUtc;

            Raise(new DriverAssignedDomainEvent(Id, driverId));

            return Result.Success();
        }

        public Result StartRide(DateTime startedAtUtc)
        {
            if (Status != RideStatus.DriverAssigned)
            {
                return Result.Failure(Errors.Ride.DriverIsNotAssigned());
            }

            Status = RideStatus.InProgress;
            StartedAtUtc = startedAtUtc;

            return Result.Success();
        }

        public Result CompleteRide(decimal actualFare, DateTime completedAtUtc)
        {
            if (Status != RideStatus.InProgress)
            {
                return Result.Failure(Errors.Ride.RideIsNotInProgress());
            }

            Status = RideStatus.Completed;
            ActualFare = actualFare;
            CompletedAtUtc = completedAtUtc;

            Raise(new RideCompletedDomainEvent(Id));

            return Result.Success();
        }

        public Result CancelRide(DateTime cancelledAtUtc)
        {
            if (Status == RideStatus.Completed)
            {
                return Result.Failure(Errors.Ride.RideCannotBeCancelled());
            }

            Status = RideStatus.Cancelled;
            CancelledAtUtc = cancelledAtUtc;

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
