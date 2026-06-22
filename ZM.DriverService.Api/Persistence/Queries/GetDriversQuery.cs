using Microsoft.EntityFrameworkCore;
using ZM.DriverService.Api.Application.UseCases.GetDrivers;

namespace ZM.DriverService.Api.Persistence.Queries
{
    public class GetDriversQuery : IGetDriversQuery
    {
        private readonly DriverDbContext _dbContext;

        public GetDriversQuery(DriverDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<GetDriversDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Drivers
                .Select(d => new GetDriversDto(
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.PhoneNumber,
                    d.Status,
                    d.CurrentRideId,
                    d.CurrentLatitude,
                    d.CurrentLongitude,
                    d.CreatedAtUtc,
                    d.LastLocationUpdateAtUtc,
                    d.LastStatusChangeAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
