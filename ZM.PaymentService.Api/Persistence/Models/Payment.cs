#nullable disable
using ZM.PaymentService.Api.Domain.Enums;

namespace ZM.PaymentService.Api.Persistence.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid RideId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}
