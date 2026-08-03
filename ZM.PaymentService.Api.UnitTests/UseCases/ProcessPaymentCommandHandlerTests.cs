using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.PaymentService.Api.Application.DateTimeProvider;
using ZM.PaymentService.Api.Application.Repository;
using ZM.PaymentService.Api.Application.UnitOfWork;
using ZM.PaymentService.Api.Application.UseCases.ProcessPayment;
using ZM.PaymentService.Api.Domain.Entities;
using ZM.PaymentService.Api.Domain.Enums;
using ZM.RideService.Api.UnitTests;

namespace ZM.PaymentService.Api.UnitTests.UseCases
{
    public class ProcessPaymentCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_ProcessesPayment(
            [Frozen] Mock<IPaymentRepository> paymentRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            ProcessPaymentCommand command,
            ProcessPaymentCommandHandler handler)
        {
            // Arrange
            var processedAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(processedAtUtc);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            paymentRepositoryMock.Verify(
                x => x.CreatePaymentAsync(
                    It.Is<Payment>(payment =>
                        payment.Id != Guid.Empty &&
                        payment.RideId == command.RideId &&
                        payment.Amount == command.Amount &&
                        payment.Status == PaymentStatus.Completed &&
                        payment.CreatedAtUtc == processedAtUtc &&
                        payment.ProcessedAtUtc == processedAtUtc),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}