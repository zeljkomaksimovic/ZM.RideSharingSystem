using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.SetDriverAvailable
{
    public record SetDriverAvailableCommand(Guid DriverId) : IRequest<Result>;
}
