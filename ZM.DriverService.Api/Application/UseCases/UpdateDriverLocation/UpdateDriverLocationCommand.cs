using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.UpdateDriverLocation
{
    public record UpdateDriverLocationCommand(Guid DriverId, double Latitude, double Longitude) : IRequest<Result>;
}
