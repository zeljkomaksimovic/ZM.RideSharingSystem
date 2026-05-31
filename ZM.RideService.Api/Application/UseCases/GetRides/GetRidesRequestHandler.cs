using MediatR;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.GetRides
{
    public class GetRidesRequestHandler : IRequestHandler<GetRidesRequest, Result<IEnumerable<GetRidesDto>>>
    {
        private readonly IGetRidesQuery _getRidesQuery;

        public GetRidesRequestHandler(IGetRidesQuery getRidesQuery)
        {
            _getRidesQuery = getRidesQuery;
        }

        public async Task<Result<IEnumerable<GetRidesDto>>> Handle(GetRidesRequest request, CancellationToken cancellationToken)
        {
            var rides = await _getRidesQuery.ExecuteAsync(cancellationToken);
            return Result<IEnumerable<GetRidesDto>>.Success(rides);
        }
    }
}
