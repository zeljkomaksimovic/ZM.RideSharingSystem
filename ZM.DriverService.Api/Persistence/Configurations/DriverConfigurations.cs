using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZM.DriverService.Api.Persistence.Models;

namespace ZM.DriverService.Api.Persistence.Configurations
{
    public class DriverConfigurations : IEntityTypeConfiguration<Models.Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<int>();
        }
    }
}
