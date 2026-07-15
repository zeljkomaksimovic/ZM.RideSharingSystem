#nullable disable
using ZM.RideService.Api.Domain.Enums;

namespace ZM.RideService.Api.Persistence.Models
{
    public class Ride
    {
        public Guid Id { get; set; }
        public Guid RiderId { get; set; }
        public string RiderFirstName { get; set; }
        public string RiderLastName { get; set; }
        public string RiderEmail { get; set; }
        public Guid? DriverId { get; set; }
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public string PickupAddress { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
        public string DestinationAddress { get; set; }
        public RideStatus Status { get; set; }
        public decimal? EstimatedFare { get; set; }
        public decimal? ActualFare { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? AssignedAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
    }
}
