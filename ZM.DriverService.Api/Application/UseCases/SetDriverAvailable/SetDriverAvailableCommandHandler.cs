using MediatR;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.SetDriverAvailable
{
    public class SetDriverAvailableCommandHandler : IRequestHandler<SetDriverAvailableCommand, Result>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SetDriverAvailableCommandHandler(
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

        public async Task<Result> Handle(SetDriverAvailableCommand request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                return Result.Failure(Errors.Driver.DriverNotFound());
            }

            var setAvailableResult = driver.SetAvailable(_dateTimeProvider.UtcNow);
            if (setAvailableResult.IsSuccessful is false)
            {
                return setAvailableResult;
            }

            await _driverRepository.UpdateDriverAsync(driver, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new DriverAvailableDomainEvent(driver.Id), cancellationToken);

            return Result.Success();
        }
    }
}
