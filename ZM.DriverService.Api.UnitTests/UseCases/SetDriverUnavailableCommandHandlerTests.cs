using AutoFixture.Xunit2;
using FluentAssertions;
using MediatR;
using Moq;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Application.UseCases.SetDriverUnavailable;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.UnitTests.Builders;
using ZM.RideService.Api.UnitTests;

namespace ZM.DriverService.Api.UnitTests.UseCases
{
    public class SetDriverUnavailableCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverExists_SetsDriverUnavailable(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IPublisher> publisherMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            SetDriverUnavailableCommand command,
            SetDriverUnavailableCommandHandler handler)
        {
            // Arrange
            var unavailableAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var driver = DriverBuilder.BuildAvailable(command.DriverId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(unavailableAtUtc);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            publisherMock
                .Setup(x => x.Publish(
                    It.IsAny<DriverUnavailableDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            driver.Status.Should().Be(DriverStatus.Offline);
            driver.LastStatusChangeAtUtc.Should().Be(unavailableAtUtc);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(
                    driver,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            publisherMock.Verify(
                x => x.Publish(
                    It.Is<DriverUnavailableDomainEvent>(e =>
                        e.DriverId == command.DriverId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_DriverNotFound_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IPublisher> publisherMock,
            SetDriverUnavailableCommand command,
            SetDriverUnavailableCommandHandler handler)
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

            publisherMock.Verify(
                x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_DriverInRide_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IPublisher> publisherMock,
            SetDriverUnavailableCommand command,
            SetDriverUnavailableCommandHandler handler)
        {
            // Arrange
            var driver = DriverBuilder.BuildInRide(command.DriverId);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Driver.DriverIsCurrentlyInRide().ErrorCode);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);

            publisherMock.Verify(
                x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}