using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Application.UseCases.SendRideCancelledNotification;

namespace ZM.NotificationService.Api.UnitTests.UseCases
{
    public class SendRideCancelledNotificationCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_SendsRideCancelledEmail(
            [Frozen] Mock<IEmailSender> emailSenderMock,
            SendRideCancelledNotificationCommand command,
            SendRideCancelledNotificationCommandHandler handler)
        {
            // Arrange
            emailSenderMock
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            emailSenderMock.Verify(
                x => x.SendEmailAsync(
                    command.RecipientEmail,
                    EmailTemplates.Ride.CancelledSubject,
                    EmailTemplates.Ride.Cancelled(command.RideId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}