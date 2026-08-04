using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Application.UseCases.SendDriverAssignedNotification;

namespace ZM.NotificationService.Api.UnitTests.UseCases
{
    public class SendDriverAssignedNotificationCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_SendsDriverAssignedEmail(
            [Frozen] Mock<IEmailSender> emailSenderMock,
            SendDriverAssignedNotificationCommand command,
            SendDriverAssignedNotificationCommandHandler handler)
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
                    EmailTemplates.Driver.AssignedSubject,
                    EmailTemplates.Driver.Assigned(command.RideId, command.DriverId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}