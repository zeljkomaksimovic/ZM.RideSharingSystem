using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.StartRide
{
    public record StartRideCommand(Guid DriverId) : IRequest<Result>;
}
