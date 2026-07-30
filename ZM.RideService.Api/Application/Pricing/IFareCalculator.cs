using ZM.RideService.Api.Domain.Entities;

namespace ZM.RideService.Api.Application.Pricing
{
    public interface IFareCalculator
    {
        Task<decimal> CalculateAsync(Ride ride, DateTime completedAtUtc, CancellationToken cancellationToken = default);
    }
}
