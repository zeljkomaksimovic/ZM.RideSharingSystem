using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Pricing;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Application.UseCases.CreateRide;
using ZM.RideService.Api.Domain.Entities;

namespace ZM.RideService.Api.UnitTests.UseCases
{
    public class CreateRideCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_CreatesRide(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IFareEstimator> fareEstimatorMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            CreateRideCommand command,
            CreateRideCommandHandler handler)
        {
            // Arrange
            var createdAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(createdAtUtc);

            fareEstimatorMock
                .Setup(x => x.EstimateAsync(
                    command.PickupLocation,
                    command.DestinationLocation,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(10m);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            fareEstimatorMock.Verify(
                x => x.EstimateAsync(
                    command.PickupLocation,
                    command.DestinationLocation,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            rideRepositoryMock.Verify(
                x => x.CreateRideAsync(
                    It.Is<Ride>(ride =>
                        ride.Rider == command.Rider &&
                        ride.DriverId == null &&
                        ride.PickupLocation == command.PickupLocation &&
                        ride.DestinationLocation == command.DestinationLocation &&
                        ride.EstimatedFare == 10m &&
                        ride.CreatedAtUtc == createdAtUtc),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}