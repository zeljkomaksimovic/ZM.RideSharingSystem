using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZM.RideService.Api.Persistence.Models;

namespace ZM.RideService.Api.Persistence.Configurations
{
    public class RideConfigurations : IEntityTypeConfiguration<Models.Ride>
    {
        public void Configure(EntityTypeBuilder<Ride> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<int>();
        }
    }
}
