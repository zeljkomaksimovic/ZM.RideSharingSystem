using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Application.Pricing
{
    public interface IFareEstimator
    {
        Task<decimal> EstimateAsync(RideLocation pickup, RideLocation destination, CancellationToken cancellationToken = default);
    }
}
