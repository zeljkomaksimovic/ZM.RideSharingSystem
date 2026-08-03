using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Application.UseCases.CompleteRide;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.UnitTests.Builders;
using ZM.RideService.Api.UnitTests;

namespace ZM.DriverService.Api.UnitTests.UseCases
{
    public class CompleteRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverInRide_CompletesRide(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            CompleteRideCommand command,
            CompleteRideCommandHandler handler)
        {
            // Arrange
            var completedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var driver = DriverBuilder.BuildInRide(command.DriverId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(completedAtUtc);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            driver.CurrentRideId.Should().BeNull();
            driver.Status.Should().Be(DriverStatus.Available);
            driver.LastStatusChangeAtUtc.Should().Be(completedAtUtc);

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
            CompleteRideCommand command,
            CompleteRideCommandHandler handler)
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
        public async Task Handle_DriverNotInRide_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            CompleteRideCommand command,
            CompleteRideCommandHandler handler)
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
            result.Error.ErrorCode.Should().Be(Errors.Driver.DriverMustBeInRide().ErrorCode);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}