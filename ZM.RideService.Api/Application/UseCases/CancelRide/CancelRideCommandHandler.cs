using MediatR;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.ErrorMessages;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.CancelRide
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

            var cancelRideResult = Ride.CancelRide(ride, _dateTimeProvider.UtcNow);
            if (cancelRideResult.IsSuccessful is false)
            {
                return cancelRideResult;
            }

            await _rideRepository.CancelRideAsync(ride, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
