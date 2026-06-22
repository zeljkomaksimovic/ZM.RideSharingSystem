namespace ZM.MatchingService.Api.Application.UseCases.GetAvailableDrivers
{
    public record GetAvailableDriversDto(Guid DriverId, double Latitude, double Longitude);
}
