using MediatR;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.AssignRide
{
    public class AssignRideCommandHandler : IRequestHandler<AssignRideCommand, Result>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AssignRideCommandHandler(
            IDriverRepository driverRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            IDateTimeProvider dateTimeProvider)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(AssignRideCommand request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                return Result.Failure(Errors.Driver.DriverNotFound());
            }

            var assignRideResult = driver.AssignRide(request.RideId, _dateTimeProvider.UtcNow);
            if (assignRideResult.IsSuccessful is false)
            {
                return assignRideResult;
            }

            await _driverRepository.UpdateDriverAsync(driver, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new RideAssignedDomainEvent(request.RideId, driver.Id), cancellationToken);

            return Result.Success();
        }
    }
}
