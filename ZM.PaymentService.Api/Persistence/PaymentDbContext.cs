using Microsoft.EntityFrameworkCore;

namespace ZM.PaymentService.Api.Persistence
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        }

        public DbSet<Models.Payment> Payments { get; set; }
        public DbSet<Idempotence.ProcessedMessage> ProcessedMessages { get; set; }
    }
}
