using Moq;
using AutoFixture.Xunit2;
using FluentAssertions;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Application.UseCases.CancelRide;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ErrorMessages;

namespace ZM.RideService.Api.UnitTests.UseCases
{
    public class CancelRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenRideExists_ShouldCancelRide(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            CancelRideCommand command,
            CancelRideCommandHandler handler)
        {
            // Arrange
            var cancelledAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var ride = RideBuilder.BuildRequested(command.RideId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(cancelledAtUtc);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            ride.Status.Should().Be(RideStatus.Cancelled);
            ride.CancelledAtUtc.Should().Be(cancelledAtUtc);

            rideRepositoryMock.Verify(
                x => x.CancelRideAsync(
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
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            CancelRideCommand command,
            CancelRideCommandHandler handler)
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
                x => x.CancelRideAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_CompletedRide_ReturnsFailure(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            CancelRideCommand command,
            CancelRideCommandHandler handler)
        {
            // Arrange
            var ride = RideBuilder.BuildCompleted(command.RideId);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Ride.RideCannotBeCancelled().ErrorCode);

            rideRepositoryMock.Verify(
                x => x.CancelRideAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}