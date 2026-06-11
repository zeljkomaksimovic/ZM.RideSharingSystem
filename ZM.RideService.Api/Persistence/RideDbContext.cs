using Microsoft.EntityFrameworkCore;
using ZM.RideService.Api.Persistence.Idempotence;
using ZM.RideService.Api.Persistence.Models;
using ZM.RideService.Api.Persistence.Outbox;

namespace ZM.RideService.Api.Persistence
{
    public class RideDbContext : DbContext
    {
        public RideDbContext(DbContextOptions<RideDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RideDbContext).Assembly);
        }

        public DbSet<Ride> Rides { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    }
}
