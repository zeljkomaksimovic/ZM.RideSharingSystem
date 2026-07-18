using MediatR;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.StartRide
{
    public record StartRideCommand(Guid RideId) : IRequest<Result>;
}
