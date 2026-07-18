using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZM.PaymentService.Api.Persistence.Idempotence;

namespace ZM.PaymentService.Api.Persistence.Configurations
{
    public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
    {
        public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
        {
            builder.HasKey(p => new { p.MessageId, p.ConsumerName });

            builder.Property(p => p.ConsumerName).HasMaxLength(200).IsRequired();

            builder.HasIndex(p => new { p.MessageId, p.ConsumerName }).IsUnique();
        }
    }
}
