using Microsoft.EntityFrameworkCore;

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

        public DbSet<Models.Ride> Rides { get; set; }
        public DbSet<Outbox.OutboxMessage> OutboxMessages { get; set; }
    }
}
