using MediatR;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.CancelRide
{
    public record CancelRideCommand(Guid RideId) : IRequest<Result>;
}
