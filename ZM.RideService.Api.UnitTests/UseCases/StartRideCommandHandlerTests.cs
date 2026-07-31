using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Application.UseCases.StartRide;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ErrorMessages;

namespace ZM.RideService.Api.UnitTests.UseCases
{
    public class StartRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_StartsRide(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            StartRideCommand command,
            StartRideCommandHandler handler)
        {
            // Arrange
            var startedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var ride = RideBuilder.BuildAssigned(command.RideId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(startedAtUtc);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            ride.Status.Should().Be(RideStatus.InProgress);
            ride.StartedAtUtc.Should().Be(startedAtUtc);

            rideRepositoryMock.Verify(
                x => x.StartRideAsync(
                    ride,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_RideNotFound_ReturnsFailure(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            StartRideCommand command,
            StartRideCommandHandler handler)
        {
            // Arrange
            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ride?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Ride.RideNotFound().ErrorCode);

            rideRepositoryMock.Verify(
                x => x.StartRideAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_RideNotAssigned_ReturnsFailure(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            StartRideCommand command,
            StartRideCommandHandler handler)
        {
            // Arrange
            var ride = RideBuilder.BuildRequested(command.RideId);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Ride.DriverIsNotAssigned().ErrorCode);

            rideRepositoryMock.Verify(
                x => x.StartRideAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}