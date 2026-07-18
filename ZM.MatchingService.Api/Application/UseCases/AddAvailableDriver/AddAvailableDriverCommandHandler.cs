using MediatR;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Domain.OperationResult;

namespace ZM.MatchingService.Api.Application.UseCases.AddAvailableDriver
{
    public class AddAvailableDriverCommandHandler : IRequestHandler<AddAvailableDriverCommand, Result>
    {
        private readonly IAvailableDriverCache _availableDriverCache;

        public AddAvailableDriverCommandHandler(IAvailableDriverCache availableDriverCache)
        {
            _availableDriverCache = availableDriverCache;
        }

        public async Task<Result> Handle(AddAvailableDriverCommand request, CancellationToken cancellationToken)
        {
            await _availableDriverCache.AddAvailableDriverAsync(request.DriverId, cancellationToken);
            return Result.Success();
        }
    }
}