#nullable disable
using ZM.DriverService.Api.Domain.Enums;

namespace ZM.DriverService.Api.Persistence.Models
{
    public class Driver
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DriverStatus Status { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? LastLocationUpdateAtUtc { get; set; }
        public DateTime? LastStatusChangeAtUtc { get; set; }
    }
}
