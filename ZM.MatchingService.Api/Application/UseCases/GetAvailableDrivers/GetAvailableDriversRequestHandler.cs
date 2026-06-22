using MediatR;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.GetAvailableDrivers
{
    public class GetAvailableDriversRequestHandler : IRequestHandler<GetAvailableDriversRequest, Result<IEnumerable<GetAvailableDriversDto>>>
    {
        private readonly IGetAvailableDriversQuery _getAvailableDriversQuery;

        public GetAvailableDriversRequestHandler(IGetAvailableDriversQuery getAvailableDriversQuery)
        {
            _getAvailableDriversQuery = getAvailableDriversQuery;
        }

        public async Task<Result<IEnumerable<GetAvailableDriversDto>>> Handle(GetAvailableDriversRequest request, CancellationToken cancellationToken)
        {
            var availableDrivers = await _getAvailableDriversQuery.ExecuteAsync(cancellationToken);
            return Result<IEnumerable<GetAvailableDriversDto>>.Success(availableDrivers);
        }
    }
}
