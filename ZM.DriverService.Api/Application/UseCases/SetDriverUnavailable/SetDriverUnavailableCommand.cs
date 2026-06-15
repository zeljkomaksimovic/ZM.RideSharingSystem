using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.SetDriverUnavailable
{
    public record SetDriverUnavailableCommand(Guid DriverId) : IRequest<Result>;
}
