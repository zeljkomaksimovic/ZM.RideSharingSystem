namespace ZM.PaymentService.Api.Persistence.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime OccurredAtUtc { get; set; }
    }
}
