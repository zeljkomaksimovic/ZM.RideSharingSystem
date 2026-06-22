using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.GetDrivers
{
    public class GetDriversRequestHandler : IRequestHandler<GetDriversRequest, Result<IEnumerable<GetDriversDto>>>
    {
        private readonly IGetDriversQuery _getDriversQuery;

        public GetDriversRequestHandler(IGetDriversQuery getDriversQuery)
        {
            _getDriversQuery = getDriversQuery;
        }

        public async Task<Result<IEnumerable<GetDriversDto>>> Handle(GetDriversRequest request, CancellationToken cancellationToken)
        {
            var drivers = await _getDriversQuery.ExecuteAsync(cancellationToken);
            return Result<IEnumerable<GetDriversDto>>.Success(drivers);
        }
    }
}
