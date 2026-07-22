using MediatR;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.Domain.OperationResult;
using ZM.DriverService.Api.Domain.ValueObjects;

namespace ZM.DriverService.Api.Application.UseCases.UpdateDriverLocation
{
    public class UpdateDriverLocationCommandHandler : IRequestHandler<UpdateDriverLocationCommand, Result>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateDriverLocationCommandHandler(
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

        public async Task<Result> Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                return Result.Failure(Errors.Driver.DriverNotFound());
            }

            driver.UpdateLocation(new DriverLocation(request.Latitude, request.Longitude), _dateTimeProvider.UtcNow);

            await _driverRepository.UpdateDriverAsync(driver, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new DriverLocationUpdatedDomainEvent(
                driver.Id, 
                request.Latitude,
                request.Longitude),
                cancellationToken);

            return Result.Success();
        }
    }
}
