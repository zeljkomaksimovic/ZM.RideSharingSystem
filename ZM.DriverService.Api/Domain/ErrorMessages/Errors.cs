namespace ZM.DriverService.Api.Domain.ErrorMessages
{
    public static class Errors
    {
        public static class Driver
        {
            public static Error DriverNotFound() => new("DRIVER0001", "Driver not found.");
            public static Error DriverIsCurrentlyInRide() => new("DRIVER0002", "Driver is currently in a ride and cannot be assigned to another.");
            public static Error DriverMustBeAvailable() => new("DRIVER0003", "Driver must be available to be assigned to a ride.");
            public static Error DriverMustBeAssigned() => new("DRIVER0004", "Driver must be assigned to start a ride.");
            public static Error DriverMustBeInRide() => new("DRIVER0005", "Driver must be in a ride to complete it.");
        }
    }
}
