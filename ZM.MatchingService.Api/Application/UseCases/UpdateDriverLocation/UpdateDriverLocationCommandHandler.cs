using MediatR;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.DateTimeProvider;
using ZM.MatchingService.Api.Domain.OperationResult;
using ZM.MatchingService.Api.Domain.Models;

namespace ZM.MatchingService.Api.Application.UseCases.UpdateDriverLocation
{
    public class UpdateDriverLocationCommandHandler : IRequestHandler<UpdateDriverLocationCommand, Result>
    {
        private readonly IAvailableDriverCache _availableDriverCache;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateDriverLocationCommandHandler(IAvailableDriverCache availableDriverCache, IDateTimeProvider dateTimeProvider)
        {
            _availableDriverCache = availableDriverCache;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
        {
            var driver = new AvailableDriver(
                request.DriverId,
                request.Latitude,
                request.Longitude,
                _dateTimeProvider.UtcNow);

            await _availableDriverCache.AddOrUpdateDriverAsync(driver, cancellationToken);

            return Result.Success();
        }
    }
}