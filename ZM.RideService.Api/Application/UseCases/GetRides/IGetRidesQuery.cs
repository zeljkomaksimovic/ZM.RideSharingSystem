namespace ZM.RideService.Api.Application.UseCases.GetRides
{
    public interface IGetRidesQuery
    {
        Task<IEnumerable<GetRidesDto>> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
