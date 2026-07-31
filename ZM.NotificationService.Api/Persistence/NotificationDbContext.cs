using Microsoft.EntityFrameworkCore;

namespace ZM.NotificationService.Api.Persistence
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        }

        public DbSet<Idempotence.ProcessedMessage> ProcessedMessages { get; set; }
    }
}
