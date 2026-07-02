using ZM.MatchingService.Api.Application.DateTimeProvider;

namespace ZM.MatchingService.Api.Infrastructure.DateTimeProviders
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
