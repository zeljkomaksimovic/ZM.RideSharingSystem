using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.Domain.OperationResult;
using ZM.DriverService.Api.Domain.Primitives;
using ZM.DriverService.Api.Domain.ValueObjects;

namespace ZM.DriverService.Api.Domain.Entities
{
    public class Driver : AggregateRoot
    {
        private Driver()
        {
        }

        private Driver(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            DriverLocation? currentLocation,
            DriverStatus status,
            DateTime createdAtUtc)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            CurrentLocation = currentLocation;
            Status = status;
            CreatedAtUtc = createdAtUtc;
        }

        public Guid Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public Guid? CurrentRideId { get; private set; }
        public DriverLocation? CurrentLocation { get; private set; }
        public DriverStatus Status { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? LastLocationUpdateAtUtc { get; private set; }
        public DateTime? LastStatusChangeAtUtc { get; private set; }

        public static Driver Create(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            DriverLocation? currentLocation,
            DateTime createdAtUtc)
        {
            return new Driver(
                id,
                firstName,
                lastName,
                email,
                phoneNumber,
                currentLocation,
                DriverStatus.Offline,
                createdAtUtc);
        }

        public Result SetAvailable(DateTime changedAtUtc)
        {
            if (Status == DriverStatus.Available)
            {
                return Result.Success();
            }

            Status = DriverStatus.Available;
            LastStatusChangeAtUtc = changedAtUtc;

            Raise(new DriverAvailableDomainEvent(Id, CurrentLocation!.Latitude, CurrentLocation!.Longitude));

            return Result.Success();
        }

        public Result SetUnavailable(DateTime changedAtUtc)
        {
            if (Status == DriverStatus.InRide)
            {
                return Result.Failure(Errors.Driver.DriverIsCurrentlyInRide());
            }

            Status = DriverStatus.Offline;
            LastStatusChangeAtUtc = changedAtUtc;

            Raise(new DriverUnavailableDomainEvent(Id));

            return Result.Success();
        }

        public Result AssignRide(Guid rideId, DateTime assignedAtUtc)
        {
            if (Status != DriverStatus.Available)
            {
                return Result.Failure(Errors.Driver.DriverMustBeAvailable());
            }

            CurrentRideId = rideId;
            Status = DriverStatus.Assigned;
            LastStatusChangeAtUtc = assignedAtUtc;

            Raise(new RideAssignedDomainEvent(rideId, Id));

            return Result.Success();
        }

        public Result StartRide(DateTime startedAtUtc)
        {
            if (Status != DriverStatus.Assigned)
            {
                return Result.Failure(Errors.Driver.DriverMustBeAssigned());
            }

            Status = DriverStatus.InRide;
            LastStatusChangeAtUtc = startedAtUtc;

            return Result.Success();
        }

        public Result CompleteRide(DateTime completedAtUtc)
        {
            if (Status != DriverStatus.InRide)
            {
                return Result.Failure(Errors.Driver.DriverMustBeInRide());
            }

            CurrentRideId = null;
            Status = DriverStatus.Available;
            LastStatusChangeAtUtc = completedAtUtc;

            return Result.Success();
        }

        public void UpdateLocation(DriverLocation location, DateTime updatedAtUtc)
        {
            CurrentLocation = location;
            LastLocationUpdateAtUtc = updatedAtUtc;
        }

        public static Driver Rehydrate(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            DriverLocation? currentLocation,
            DriverStatus status,
            DateTime createdAtUtc,
            DateTime? lastLocationUpdateAtUtc,
            DateTime? lastStatusChangeAtUtc)
        {
            return new Driver
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                CurrentLocation = currentLocation,
                Status = status,
                CreatedAtUtc = createdAtUtc,
                LastLocationUpdateAtUtc = lastLocationUpdateAtUtc,
                LastStatusChangeAtUtc = lastStatusChangeAtUtc
            };
        }
    }
}
