using ZM.RideService.Api.Application.DateTimeProvider;

namespace ZM.RideService.Api.Infrastructure.DateTimeProviders
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
