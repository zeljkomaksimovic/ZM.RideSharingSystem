using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.CompleteRide
{
    public record CompleteRideCommand(Guid DriverId) : IRequest<Result>;
}
