using MediatR;
using ZM.MatchingService.Api.Domain.OperationResult;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Application.UseCases.UpdateDriverLocation
{
    public record UpdateDriverLocationCommand(Guid DriverId, GeoLocation Location) : IRequest<Result>;
}
