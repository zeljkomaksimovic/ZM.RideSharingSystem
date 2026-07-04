namespace ZM.PaymentService.Api.Persistence.Idempotence
{
    public class ProcessedMessage
    {
        public Guid? MessageId { get; set; }
        public string ConsumerName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
