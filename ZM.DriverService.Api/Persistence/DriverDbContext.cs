using Microsoft.EntityFrameworkCore;
using ZM.DriverService.Api.Persistence.Idempotence;
using ZM.DriverService.Api.Persistence.Models;

namespace ZM.DriverService.Api.Persistence
{
    public class DriverDbContext : DbContext
    {
        public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriverDbContext).Assembly);
        }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    }
}
