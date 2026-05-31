namespace ZM.RideService.Api.Domain.Errors
{
    public static class Errors
    {
        public static class Ride
        {
            public static Error RideNotFound() => new("RIDE0001", "Ride not found");
        }
    }
}
