using Microsoft.EntityFrameworkCore;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ValueObjects;

namespace ZM.DriverService.Api.Persistence.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly DriverDbContext _dbContext;

        public DriverRepository(DriverDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateDriverAsync(Driver driver, CancellationToken cancellationToken = default)
        {
            var dbDriver = new Models.Driver
            {
                Id = driver.Id,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                Email = driver.Email,
                PhoneNumber = driver.PhoneNumber,
                CurrentLatitude = driver.CurrentLocation?.Latitude,
                CurrentLongitude = driver.CurrentLocation?.Longitude,
                Status = driver.Status,
                CreatedAtUtc = driver.CreatedAtUtc,
                LastLocationUpdateAtUtc = driver.LastLocationUpdateAtUtc,
                LastStatusChangeAtUtc = driver.LastStatusChangeAtUtc
            };

            await _dbContext.Drivers.AddAsync(dbDriver, cancellationToken);
        }

        public async Task<Driver?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Drivers
                .Where(d => d.Id == driverId)
                .Select(d => Driver.Rehydrate(
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.PhoneNumber,
                    d.CurrentLatitude.HasValue && d.CurrentLongitude.HasValue ?
                        new DriverLocation(d.CurrentLatitude.Value, d.CurrentLongitude.Value) : null,
                    d.Status,
                    d.CreatedAtUtc,
                    d.LastLocationUpdateAtUtc,
                    d.LastStatusChangeAtUtc))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> DriverExistsAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Drivers.AnyAsync(d => d.Id == driverId, cancellationToken);
        }

        public async Task<bool> UpdateDriverAsync(Driver driver, CancellationToken cancellationToken = default)
        {
            var dbDriver = await _dbContext.Drivers.SingleOrDefaultAsync(d => d.Id == driver.Id, cancellationToken);

            if (dbDriver is null)
            {
                return false;
            }

            dbDriver.FirstName = driver.FirstName;
            dbDriver.LastName = driver.LastName;
            dbDriver.Email = driver.Email;
            dbDriver.PhoneNumber = driver.PhoneNumber;
            dbDriver.Status = driver.Status;
            dbDriver.CurrentRideId = driver.CurrentRideId;
            dbDriver.CurrentLatitude = driver.CurrentLocation?.Latitude;
            dbDriver.CurrentLongitude = driver.CurrentLocation?.Longitude;
            dbDriver.LastLocationUpdateAtUtc = driver.LastLocationUpdateAtUtc;
            dbDriver.LastStatusChangeAtUtc = driver.LastStatusChangeAtUtc;

            return true;
        }

        public async Task<IReadOnlyCollection<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Drivers
                .AsNoTracking()
                .Where(d => d.Status == DriverStatus.Available)
                .Select(d => Driver.Rehydrate(
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.PhoneNumber,
                    d.CurrentLatitude.HasValue && d.CurrentLongitude.HasValue ?
                        new DriverLocation(d.CurrentLatitude.Value, d.CurrentLongitude.Value) : null,
                    d.Status,
                    d.CreatedAtUtc,
                    d.LastLocationUpdateAtUtc,
                    d.LastStatusChangeAtUtc))
                .ToListAsync(cancellationToken);
        }
    }
}
