using MediatR;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.RemoveAvailableDriver
{
    public record RemoveAvailableDriverCommand(Guid DriverId) : IRequest<Result>;
}
