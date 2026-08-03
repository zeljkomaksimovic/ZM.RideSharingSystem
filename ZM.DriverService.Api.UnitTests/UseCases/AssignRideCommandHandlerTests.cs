using AutoFixture.Xunit2;
using FluentAssertions;
using MediatR;
using Moq;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Application.UseCases.AssignRide;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.UnitTests.Builders;
using ZM.RideService.Api.UnitTests;

namespace ZM.DriverService.Api.UnitTests.UseCases
{
    public class AssignRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverAvailable_AssignsRide(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IPublisher> publisherMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            AssignRideCommand command,
            AssignRideCommandHandler handler)
        {
            // Arrange
            var assignedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var driver = DriverBuilder.BuildAvailable(command.DriverId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(assignedAtUtc);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            publisherMock
                .Setup(x => x.Publish(It.IsAny<RideAssignedDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            driver.CurrentRideId.Should().Be(command.RideId);
            driver.Status.Should().Be(DriverStatus.Assigned);
            driver.LastStatusChangeAtUtc.Should().Be(assignedAtUtc);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(driver, It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            publisherMock.Verify(
                x => x.Publish(
                    It.Is<RideAssignedDomainEvent>(e =>
                        e.RideId == command.RideId &&
                        e.DriverId == command.DriverId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_DriverNotFound_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            AssignRideCommand command,
            AssignRideCommandHandler handler)
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
        public async Task Handle_DriverNotAvailable_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            AssignRideCommand command,
            AssignRideCommandHandler handler)
        {
            // Arrange
            var driver = DriverBuilder.BuildOffline(command.DriverId);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Driver.DriverMustBeAvailable().ErrorCode);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}