using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.GetDrivers
{
    public record GetDriversRequest : IRequest<Result<IEnumerable<GetDriversDto>>>;
}
