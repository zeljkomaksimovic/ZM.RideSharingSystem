using MediatR;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.AddAvailableDriver
{
    public record AddAvailableDriverCommand(Guid DriverId, double Latitude, double Longitude) : IRequest<Result>;
}
