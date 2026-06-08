using Microsoft.EntityFrameworkCore;
using ZM.RideService.Api.Application.UseCases.GetRides;
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Persistence.Queries
{
    public class GetRidesQuery : IGetRidesQuery
    {
        private readonly RideDbContext _dbContext;

        public GetRidesQuery(RideDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<GetRidesDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Rides
                .Select(r => new GetRidesDto(
                    r.Id,
                    r.RiderId,
                    r.DriverId,
                    new RideLocation(r.PickupLatitude, r.PickupLongitude, r.PickupAddress),
                    new RideLocation(r.DestinationLatitude, r.DestinationLongitude, r.DestinationAddress),
                    r.Status,
                    r.EstimatedFare,
                    r.ActualFare,
                    r.CreatedAtUtc,
                    r.AssignedAtUtc,
                    r.StartedAtUtc,
                    r.CompletedAtUtc,
                    r.CancelledAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}