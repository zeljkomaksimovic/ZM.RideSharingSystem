#nullable disable
using Newtonsoft.Json;
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

        private Ride(Guid rideId, RiderInfo rider, RideLocation pickupLocation, RideLocation destinationLocation, RideStatus status, decimal? estimatedFare, DateTime createdAtUtc)
        {
            Id = rideId;
            Rider = rider;
            PickupLocation = pickupLocation;
            DestinationLocation = destinationLocation;
            Status = status;
            EstimatedFare = estimatedFare;
            CreatedAtUtc = createdAtUtc;
        }

        private Ride(
            Guid id,
            RiderInfo rider,
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
            Rider = rider;
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
        public RiderInfo Rider { get; private set; }
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

        public static Ride Create(Guid rideId, RiderInfo rider, RideLocation pickupLocation, RideLocation destinationLocation, decimal? estimatedFare, DateTime createdAtUtc)
        {
            var ride = new Ride(
                rideId,
                rider,
                pickupLocation,
                destinationLocation,
                RideStatus.Requested,
                estimatedFare,
                createdAtUtc);

            ride.Raise(new RideCreatedDomainEvent(
                ride.Id, 
                ride.PickupLocation.Latitude,
                ride.PickupLocation.Longitude, 
                ride.Rider));

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

            Raise(new DriverAssignedDomainEvent(Id, DriverId.Value));

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

            Raise(new RideStartedDomainEvent(Id));

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

            Raise(new RideCompletedDomainEvent(Id, Rider));

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
            RiderInfo rider,
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
                rider,
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
