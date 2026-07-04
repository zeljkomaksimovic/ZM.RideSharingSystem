using ZM.PaymentService.Api.Application.DateTimeProvider;

namespace ZM.PaymentService.Api.Infrastructure.DateTimeProviders
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
