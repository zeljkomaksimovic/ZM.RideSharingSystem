using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.NotificationService.Api.Application.NotificationSender;
using ZM.NotificationService.Api.Application.Templates;
using ZM.NotificationService.Api.Application.UseCases.SendPaymentReceiptNotification;

namespace ZM.NotificationService.Api.UnitTests.UseCases
{
    public class SendPaymentReceiptNotificationCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_SendsPaymentReceiptEmail(
            [Frozen] Mock<IEmailSender> emailSenderMock,
            SendPaymentReceiptNotificationCommand command,
            SendPaymentReceiptNotificationCommandHandler handler)
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
                    EmailTemplates.Payment.ReceiptSubject,
                    EmailTemplates.Payment.Receipt(
                        command.PaymentId,
                        command.RideId,
                        command.Amount),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}