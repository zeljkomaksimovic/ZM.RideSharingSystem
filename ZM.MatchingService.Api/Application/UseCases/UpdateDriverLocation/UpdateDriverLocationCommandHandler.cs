using MediatR;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.DateTimeProvider;
using ZM.MatchingService.Api.Domain.OperationResult;

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
            await _availableDriverCache.UpdateDriverLocationAsync(
                request.DriverId,
                request.Location,
                _dateTimeProvider.UtcNow,
                cancellationToken);

            return Result.Success();
        }
    }
}