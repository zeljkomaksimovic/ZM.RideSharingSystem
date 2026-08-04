using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Application.UseCases.AddAvailableDriver;

namespace ZM.MatchingService.Api.UnitTests.UseCases
{
    public class AddAvailableDriverCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_AddsAvailableDriver(
            [Frozen] Mock<IAvailableDriverCache> availableDriverCacheMock,
            AddAvailableDriverCommand command,
            AddAvailableDriverCommandHandler handler)
        {
            // Arrange
            availableDriverCacheMock
                .Setup(x => x.AddAvailableDriverAsync(
                    command.DriverId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            availableDriverCacheMock.Verify(
                x => x.AddAvailableDriverAsync(
                    command.DriverId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}