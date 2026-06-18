using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.AssignRide
{
    public record AssignRideCommand(Guid RideId, Guid DriverId) : IRequest<Result>;
}
