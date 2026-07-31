#nullable disable
namespace ZM.NotificationService.Api.Persistence.Idempotence
{
    public sealed class ProcessedMessage
    {
        public Guid? MessageId { get; set; }
        public string ConsumerName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
