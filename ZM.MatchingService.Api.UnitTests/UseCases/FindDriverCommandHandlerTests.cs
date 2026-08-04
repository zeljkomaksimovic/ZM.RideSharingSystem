using AutoFixture.Xunit2;
using FluentAssertions;
using MassTransit;
using Moq;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.UseCases.FindDriver;
using ZM.MatchingService.Api.Domain.Models;
using ZM.MatchingService.Api.UnitTests.Builders;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.MatchingService.Api.UnitTests.UseCases
{
    public class FindDriverCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverFound_PublishesDriverMatchedEvent(
            [Frozen] Mock<IAvailableDriverCache> availableDriverCacheMock,
            [Frozen] Mock<IPublishEndpoint> publishEndpointMock,
            FindDriverCommand command,
            FindDriverCommandHandler handler)
        {
            // Arrange
            var matchedDriver = AvailableDriverBuilder.Build(
                latitude: command.PickupLocation.Latitude,
                longitude: command.PickupLocation.Longitude);

            availableDriverCacheMock
                .Setup(x => x.GetNearestDriverAsync(
                    command.PickupLocation,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(matchedDriver);

            publishEndpointMock
                .Setup(x => x.Publish(
                    It.IsAny<DriverMatchedEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            availableDriverCacheMock.Verify(
                x => x.GetNearestDriverAsync(
                    command.PickupLocation,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            publishEndpointMock.Verify(
                x => x.Publish(
                    It.Is<DriverMatchedEvent>(e =>
                        e.RideId == command.RideId &&
                        e.DriverId == matchedDriver.DriverId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            publishEndpointMock.Verify(
                x => x.Publish(
                    It.IsAny<DriverNotFoundEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_WhenDriverNotFound_PublishesDriverNotFoundEvent(
            [Frozen] Mock<IAvailableDriverCache> availableDriverCacheMock,
            [Frozen] Mock<IPublishEndpoint> publishEndpointMock,
            FindDriverCommand command,
            FindDriverCommandHandler handler)
        {
            // Arrange
            availableDriverCacheMock
                .Setup(x => x.GetNearestDriverAsync(
                    command.PickupLocation,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AvailableDriver?)null);

            publishEndpointMock
                .Setup(x => x.Publish(
                    It.IsAny<DriverNotFoundEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            availableDriverCacheMock.Verify(
                x => x.GetNearestDriverAsync(
                    command.PickupLocation,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            publishEndpointMock.Verify(
                x => x.Publish(
                    It.Is<DriverNotFoundEvent>(e =>
                        e.RideId == command.RideId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            publishEndpointMock.Verify(
                x => x.Publish(
                    It.IsAny<DriverMatchedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}