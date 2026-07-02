using MediatR;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.UpdateDriverLocation
{
    public record UpdateDriverLocationCommand(Guid DriverId, double Latitude, double Longitude) : IRequest<Result>;
}
