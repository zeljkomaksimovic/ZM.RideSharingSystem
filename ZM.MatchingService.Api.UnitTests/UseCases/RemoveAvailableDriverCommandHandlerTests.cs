using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.UseCases.RemoveAvailableDriver;

namespace ZM.MatchingService.Api.UnitTests.UseCases
{
    public class RemoveAvailableDriverCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_RemovesAvailableDriver(
            [Frozen] Mock<IAvailableDriverCache> availableDriverCacheMock,
            RemoveAvailableDriverCommand command,
            RemoveAvailableDriverCommandHandler handler)
        {
            // Arrange
            availableDriverCacheMock
                .Setup(x => x.RemoveDriverAsync(
                    command.DriverId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            availableDriverCacheMock.Verify(
                x => x.RemoveDriverAsync(
                    command.DriverId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}