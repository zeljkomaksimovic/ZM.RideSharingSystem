namespace ZM.MatchingService.Api.Domain.ErrorMessages
{
    public static class Errors
    {
        public static class Matching
        {
            public static Error DriverNotFound => new Error("MATCHING0001", "Driver not found.");
            public static Error RideNotFound => new Error("MATCHING0002", "Ride not found.");
            public static Error DriverAlreadyAssigned => new Error("MATCHING0003", "Driver already assigned to the ride.");
        }
    }
}
