using Moq;
using AutoFixture.Xunit2;
using FluentAssertions;
using ZM.RideService.Api.Application.DateTimeProvider;
using ZM.RideService.Api.Application.Repository;
using ZM.RideService.Api.Application.UnitOfWork;
using ZM.RideService.Api.Application.UseCases.AssignDriver;
using ZM.RideService.Api.Domain.Entities;
using ZM.RideService.Api.Domain.Enums;
using ZM.RideService.Api.Domain.ErrorMessages;

namespace ZM.RideService.Api.UnitTests.UseCases
{
    public class AssignDriverCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_AssignsDriver(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            AssignDriverCommand command,
            AssignDriverCommandHandler handler)
        {
            // Arrange
            var assignedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            var ride = RideBuilder.BuildRequested(command.RideId);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(assignedAtUtc);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            ride.DriverId.Should().Be(command.DriverId);
            ride.Status.Should().Be(RideStatus.DriverAssigned);
            ride.AssignedAtUtc.Should().Be(assignedAtUtc);

            rideRepositoryMock.Verify(
                x => x.AssignDriverAsync(
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
            AssignDriverCommand command,
            AssignDriverCommandHandler handler)
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
                x => x.AssignDriverAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_RideAlreadyAssigned_ReturnsFailure(
            [Frozen] Mock<IRideRepository> rideRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            AssignDriverCommand command,
            AssignDriverCommandHandler handler)
        {
            // Arrange
            var ride = RideBuilder.BuildAssigned(command.RideId);

            rideRepositoryMock
                .Setup(x => x.GetRideByIdAsync(command.RideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Error.ErrorCode.Should().Be(Errors.Ride.DriverCouldNotBeAssigned().ErrorCode);

            rideRepositoryMock.Verify(
                x => x.AssignDriverAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}