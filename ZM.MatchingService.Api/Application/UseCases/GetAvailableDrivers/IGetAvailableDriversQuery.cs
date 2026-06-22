namespace ZM.MatchingService.Api.Application.UseCases.GetAvailableDrivers
{
    public interface IGetAvailableDriversQuery
    {
        Task<IEnumerable<GetAvailableDriversDto>> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
