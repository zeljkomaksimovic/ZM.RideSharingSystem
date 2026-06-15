using MediatR;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.ErrorMessages;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.CompleteRide
{
    public record CompleteRideCommandHandler : IRequestHandler<CompleteRideCommand, Result>
    {
        private readonly IRideRepository _rideRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CompleteRideCommandHandler(IRideRepository rideRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _rideRepository = rideRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(CompleteRideCommand request, CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetRideByIdAsync(request.RideId, cancellationToken);
            if (ride is null)
            {
                return Result.Failure(Errors.Ride.RideNotFound());
            }

            var completeRideResult = ride.CompleteRide(request.ActualFare, _dateTimeProvider.UtcNow);
            if (completeRideResult.IsSuccessful is false)
            {
                return completeRideResult;
            }

            await _rideRepository.CompleteRideAsync(ride, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
