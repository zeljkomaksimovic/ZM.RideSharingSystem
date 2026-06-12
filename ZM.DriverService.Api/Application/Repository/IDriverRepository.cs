using ZM.DriverService.Api.Domain.Entities;

namespace ZM.DriverService.Api.Application.Repository
{
    public interface IDriverRepository
    {
        Task CreateDriverAsync(Driver driver, CancellationToken cancellationToken = default);

        Task<Driver?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default);

        Task<bool> UpdateDriverAsync(Driver driver, CancellationToken cancellationToken = default);

        Task<bool> DriverExistsAsync(Guid driverId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default);
    }
}
