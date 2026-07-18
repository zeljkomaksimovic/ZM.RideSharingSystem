using MediatR;
using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.Domain.OperationResult;
using ZM.MatchingService.Api.Domain.ValueObjects;

namespace ZM.MatchingService.Api.Application.UseCases.FindDriver
{
    public record FindDriverCommand(Guid RideId, GeoLocation PickupLocation) : IRequest<Result<IEnumerable<AvailableDriver>>>;
}
