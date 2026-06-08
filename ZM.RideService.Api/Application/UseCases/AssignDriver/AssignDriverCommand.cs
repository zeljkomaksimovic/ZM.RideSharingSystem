using MediatR;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.AssignDriver
{
    public record AssignDriverCommand(Guid RideId, Guid DriverId) : IRequest<Result>;
}
