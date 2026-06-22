namespace ZM.DriverService.Api.Application.UseCases.GetDrivers
{
    public interface IGetDriversQuery
    {
        Task<IEnumerable<GetDriversDto>> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
