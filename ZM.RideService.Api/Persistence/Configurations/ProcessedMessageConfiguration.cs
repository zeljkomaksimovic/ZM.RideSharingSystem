using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZM.RideService.Api.Persistence.Idempotence;

namespace ZM.RideService.Api.Persistence.Configurations
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
