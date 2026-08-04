using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.DateTimeProvider;
using ZM.MatchingService.Api.Application.UseCases.UpdateDriverLocation;

namespace ZM.MatchingService.Api.UnitTests.UseCases
{
    public class UpdateDriverLocationCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_UpdatesDriverLocation(
            [Frozen] Mock<IAvailableDriverCache> availableDriverCacheMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            UpdateDriverLocationCommand command,
            UpdateDriverLocationCommandHandler handler)
        {
            // Arrange
            var updatedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(updatedAtUtc);

            availableDriverCacheMock
                .Setup(x => x.UpdateDriverLocationAsync(
                    command.DriverId,
                    command.Location,
                    updatedAtUtc,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            availableDriverCacheMock.Verify(
                x => x.UpdateDriverLocationAsync(
                    command.DriverId,
                    command.Location,
                    updatedAtUtc,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}