using MediatR;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.GetAvailableDrivers
{
    public record GetAvailableDriversRequest() : IRequest<Result<IEnumerable<GetAvailableDriversDto>>>;
}
