using MediatR;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.RegisterDriver
{
    public record RegisterDriverCommand(string FirstName,string LastName, string Email, string PhoneNumber) : IRequest<Result>;
}
