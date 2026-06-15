using MediatR;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.OperationResult;

namespace ZM.DriverService.Api.Application.UseCases.RegisterDriver
{
    public class RegisterDriverCommandHandler : IRequestHandler<RegisterDriverCommand, Result>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RegisterDriverCommandHandler(IDriverRepository driverRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = Driver.Create(
                Guid.NewGuid(),
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                null,
                _dateTimeProvider.UtcNow);

            await _driverRepository.CreateDriverAsync(driver, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
