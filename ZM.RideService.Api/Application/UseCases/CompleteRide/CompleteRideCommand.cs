using MediatR;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.CompleteRide
{
    public record CompleteRideCommand(Guid RideId) : IRequest<Result>;
}
