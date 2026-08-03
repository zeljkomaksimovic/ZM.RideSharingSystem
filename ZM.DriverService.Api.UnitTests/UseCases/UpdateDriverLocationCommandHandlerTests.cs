using AutoFixture.Xunit2;
using FluentAssertions;
using MediatR;
using Moq;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Application.UseCases.UpdateDriverLocation;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.ErrorMessages;
using ZM.DriverService.Api.Domain.Events;
using ZM.DriverService.Api.Domain.ValueObjects;
using ZM.DriverService.Api.UnitTests.Builders;
using ZM.RideService.Api.UnitTests;

namespace ZM.DriverService.Api.UnitTests.UseCases
{
    public class UpdateDriverLocationCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverExists_UpdatesLocation(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IPublisher> publisherMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            UpdateDriverLocationCommand command,
            UpdateDriverLocationCommandHandler handler)
        {
            // Arrange
            var updatedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var driver = DriverBuilder.BuildAvailable(command.DriverId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(updatedAtUtc);

            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(command.DriverId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(driver);

            publisherMock
                .Setup(x => x.Publish(
                    It.IsAny<DriverLocationUpdatedDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            driver.CurrentLocation.Should().Be(new DriverLocation(
                command.Latitude,
                command.Longitude));

            driver.LastLocationUpdateAtUtc.Should().Be(updatedAtUtc);

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
                    It.Is<DriverLocationUpdatedDomainEvent>(e =>
                        e.DriverId == command.DriverId &&
                        e.Latitude == command.Latitude &&
                        e.Longitude == command.Longitude),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_DriverNotFound_ReturnsFailure(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IPublisher> publisherMock,
            UpdateDriverLocationCommand command,
            UpdateDriverLocationCommandHandler handler)
        {
            // Arrange
            driverRepositoryMock
                .Setup(x => x.GetDriverByIdAsync(
                    command.DriverId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Driver?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(
                Errors.Driver.DriverNotFound().ErrorCode);

            driverRepositoryMock.Verify(
                x => x.UpdateDriverAsync(
                    It.IsAny<Driver>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);

            publisherMock.Verify(
                x => x.Publish(
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}