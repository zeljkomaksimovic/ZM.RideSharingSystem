using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.SetDriverAvailable
{
    public record SetDriverAvailableCommand(Guid DriverId, double Latitude, double Longitude) : IRequest<Result>;
}
