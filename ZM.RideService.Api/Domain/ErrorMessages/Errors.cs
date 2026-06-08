namespace ZM.RideService.Api.Domain.ErrorMessages
{
    public static class Errors
    {
        public static class Ride
        {
            public static Error RideNotFound() => new("RIDE0001", "Ride not found.");
            public static Error DriverCouldNotBeAssigned() => new("RIDE0002", "Driver could not be assigned to the ride because it is not in the requested status.");
            public static Error DriverIsNotAssigned() => new("RIDE0003", "Ride could not be started because it is not in the driver assigned status.");
            public static Error RideIsNotInProgress() => new("RIDE0004", "Ride could not be completed because it is not in the in progress status.");
            public static Error RideCannotBeCancelled() => new("RIDE0005", "Ride could not be cancelled because it is in the completed status.");
        }
    }
}
