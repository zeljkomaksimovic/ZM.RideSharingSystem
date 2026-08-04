using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Application.UseCases.SendRideCompletedNotification;

namespace ZM.NotificationService.Api.UnitTests.UseCases
{
    public class SendRideCompletedNotificationCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_SendsRideCompletedEmail(
            [Frozen] Mock<IEmailSender> emailSenderMock,
            SendRideCompletedNotificationCommand command,
            SendRideCompletedNotificationCommandHandler handler)
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
                    EmailTemplates.Ride.CompletedSubject,
                    EmailTemplates.Ride.Completed(command.RideId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}