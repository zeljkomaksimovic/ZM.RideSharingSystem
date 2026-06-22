using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZM.MatchingService.Api.Persistence.Models;

namespace ZM.MatchingService.Api.Persistence.Configurations
{
    public class AvailableDriverConfigurations : IEntityTypeConfiguration<AvailableDriver>
    {
        public void Configure(EntityTypeBuilder<AvailableDriver> builder)
        {
            builder.HasKey(x => x.DriverId);
        }
    }
}
