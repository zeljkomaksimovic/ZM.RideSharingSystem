using MassTransit;
using MediatR;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Domain.OperationResult;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.Application.UseCases.FindDriver
{
    public class FindDriverCommandHandler : IRequestHandler<FindDriverCommand, Result>
    {
        private readonly IAvailableDriverCache _availableDriverCache;
        private readonly IPublishEndpoint _publishEndpoint;

        public FindDriverCommandHandler(IAvailableDriverCache availableDriverCache, IPublishEndpoint publishEndpoint)
        {
            _availableDriverCache = availableDriverCache;
            _publishEndpoint = publishEndpoint;
        }
        //TODO: Implement the logic to find the nearest available driver based on the pickup location and publish the appropriate events.
        public async Task<Result> Handle(FindDriverCommand request, CancellationToken cancellationToken)
        {
            var matchedDriver = await _availableDriverCache.GetNearestDriverAsync(request.PickupLocation, cancellationToken);

            if (matchedDriver is null)
            {
                await _publishEndpoint.Publish(new DriverNotFoundEvent(request.RideId), cancellationToken);

                return Result.Success();
            }

            await _publishEndpoint.Publish(new DriverMatchedEvent(
                request.RideId,
                matchedDriver.DriverId),
                cancellationToken);

            return Result.Success();
        }
    }
}