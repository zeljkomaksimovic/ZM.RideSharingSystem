using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Application.UseCases.StartRide;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.UnitTests.Builders;
using ZM.RideService.Api.UnitTests;

namespace ZM.DriverService.Api.UnitTests.UseCases
{
    public class StartRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverAssigned_StartsRide(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            StartRideCommand command,
            StartRideCommandHandler handler)
        {
            // Arrange
            var startedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var driver = DriverBuilder.BuildAssigned(command.DriverId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(startedAtUtc);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            driver.Status.Should().Be(DriverStatus.InRide);
            driver.LastStatusChangeAtUtc.Should().Be(startedAtUtc);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(
                    driver,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_DriverNotFound_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            StartRideCommand command,
            StartRideCommandHandler handler)
        {
            // Arrange
            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Driver?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Driver.DriverNotFound().ErrorCode);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_DriverNotAssigned_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            StartRideCommand command,
            StartRideCommandHandler handler)
        {
            // Arrange
            var driver = DriverBuilder.BuildAvailable(command.DriverId);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Driver.DriverMustBeAssigned().ErrorCode);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}