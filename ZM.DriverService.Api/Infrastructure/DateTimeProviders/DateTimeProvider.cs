using ZM.DriverService.Api.Application.DateTimeProvider;

namespace ZM.DriverService.Api.Infrastructure.DateTimeProviders
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
