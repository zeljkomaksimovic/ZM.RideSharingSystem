using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.AssignRideToDriver
{
    public record AssignRideToDriverCommand(Guid DriverId) : IRequest<Result>;
}
