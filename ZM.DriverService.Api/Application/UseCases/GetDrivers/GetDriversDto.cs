using ZM.DriverService.Api.Domain.Enums;

namespace ZM.DriverService.Api.Application.UseCases.GetDrivers
{
    public record GetDriversDto(
    Guid Id, 
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DriverStatus Status,
    Guid? CurrentRideId,
    double? CurrentLatitude,
    double? CurrentLongitude,
    DateTime CreatedAtUtc,
    DateTime? LastLocationUpdateAtUtc,
    DateTime? LastStatusChangeAtUtc);
}
