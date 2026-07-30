using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.CompleteRide
{
    public class CompleteRideCommandHandler
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CompleteRideCommandHandler(IDriverRepository driverRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(CompleteRideCommand request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                return Result.Failure(Errors.Driver.DriverNotFound());
            }

            var completeRideResult = driver.CompleteRide(_dateTimeProvider.UtcNow);
            if (completeRideResult.IsSuccessful is false)
            {
                return completeRideResult;
            }

            await _driverRepository.UpdateDriverAsync(driver, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
