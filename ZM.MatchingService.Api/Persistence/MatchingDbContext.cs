using Microsoft.EntityFrameworkCore;
using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Persistence.Idempotence;

namespace ZM.MatchingService.Api.Persistence
{
    public class MatchingDbContext : DbContext
    {
        public MatchingDbContext(DbContextOptions<MatchingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MatchingDbContext).Assembly);
        }

        public DbSet<AvailableDriver> AvailableDrivers { get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    }
}
