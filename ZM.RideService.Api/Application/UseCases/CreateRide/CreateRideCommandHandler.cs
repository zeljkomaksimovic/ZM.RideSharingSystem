using MediatR;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.OperationResult;

namespace ZM.RideService.Api.Application.UseCases.CreateRide
{
    public class CreateRideCommandHandler : IRequestHandler<CreateRideCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRideRepository _rideRepository;

        public CreateRideCommandHandler(IRideRepository rideRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _rideRepository = rideRepository;
        }

        public async Task<Result> Handle(CreateRideCommand request, CancellationToken cancellationToken)
        {
            var ride = Ride.Create(
                Guid.NewGuid(),
                request.RiderId, 
                request.PickupLocation,
                request.DestinationLocation, 
                _dateTimeProvider.UtcNow);

            await _rideRepository.CreateRideAsync(ride, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
