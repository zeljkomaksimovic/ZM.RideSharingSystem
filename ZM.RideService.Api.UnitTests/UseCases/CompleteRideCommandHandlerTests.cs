using Moq;
using AutoFixture.Xunit2;
using FluentAssertions;
using ZM.RideService.Api.Application.Pricing;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Application.UseCases.CompleteRide;
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ErrorMessages;

namespace ZM.RideService.Api.UnitTests.UseCases
{
    public class CompleteRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_CompletesRide(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IFareCalculator> fareCalculatorMock,
            CompleteRideCommand command,
            CompleteRideCommandHandler handler)
        {
            // Arrange
            var ride = RideBuilder.BuildInProgress(command.RideId);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            fareCalculatorMock
                .Setup(x => x.CalculateAsync(
                    ride,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(15.5m);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            ride.Status.Should().Be(RideStatus.Completed);
            ride.ActualFare.Should().Be(15.5m);
            ride.CompletedAtUtc.Should().NotBeNull();

            fareCalculatorMock.Verify(
                x => x.CalculateAsync(
                    ride,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            rideRepositoryMock.Verify(
                x => x.CompleteRideAsync(
                    ride,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_RideNotInProgress_ReturnsFailure(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            CompleteRideCommand command,
            CompleteRideCommandHandler handler)
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
            result.Error.ErrorCode.Should().Be(
                Errors.Ride.RideIsNotInProgress().ErrorCode);
        }
    }
}