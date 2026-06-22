using Microsoft.EntityFrameworkCore;
using ZM.MatchingService.Api.Application.UseCases.GetAvailableDrivers;

namespace ZM.MatchingService.Api.Persistence.Queries
{
    public class GetAvailableDriversQuery : IGetAvailableDriversQuery
    {
        private readonly MatchingDbContext _dbContext;

        public GetAvailableDriversQuery(MatchingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<GetAvailableDriversDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.AvailableDrivers
                .Select(d => new GetAvailableDriversDto(
                    d.DriverId, 
                    d.Latitude, 
                    d.Longitude))
                .ToListAsync(cancellationToken);
        }
    }
}
