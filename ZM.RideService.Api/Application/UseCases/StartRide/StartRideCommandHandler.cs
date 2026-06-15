using MediatR;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.ErrorMessages;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.StartRide
{
    public class CancelRideCommandHandler : IRequestHandler<CancelRideCommand, Result>
    {
        private readonly IRideRepository _rideRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CancelRideCommandHandler(IRideRepository rideRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _rideRepository = rideRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(CancelRideCommand request, CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetRideByIdAsync(request.RideId, cancellationToken);
            if (ride is null)
            {
                return Result.Failure(Errors.Ride.RideNotFound());
            }

            var startRideResult = ride.StartRide(_dateTimeProvider.UtcNow);
            if (startRideResult.IsSuccessful is false)
            {
                return startRideResult;
            }

            await _rideRepository.StartRideAsync(ride, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
