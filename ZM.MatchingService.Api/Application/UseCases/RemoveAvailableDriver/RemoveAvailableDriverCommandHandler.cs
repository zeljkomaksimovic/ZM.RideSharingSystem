using MediatR;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.UseCases.SetDriverUnavailable;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.RemoveAvailableDriver
{
    public class RemoveAvailableDriverCommandHandler : IRequestHandler<RemoveAvailableDriverCommand, Result>
    {
        private readonly IAvailableDriverCache _availableDriverCache;

        public RemoveAvailableDriverCommandHandler(IAvailableDriverCache availableDriverCache)
        {
            _availableDriverCache = availableDriverCache;
        }

        public async Task<Result> Handle(RemoveAvailableDriverCommand request, CancellationToken cancellationToken)
        {
            await _availableDriverCache.RemoveDriverAsync(request.DriverId, cancellationToken);
            return Result.Success();
        }
    }
}