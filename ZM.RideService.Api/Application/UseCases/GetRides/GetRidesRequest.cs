using MediatR;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.GetRides
{
    public record GetRidesRequest : IRequest<Result<IEnumerable<GetRidesDto>>>;
}
