using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ZM.DriverService.Api.Application.DateTimeProvider;
using ZM.DriverService.Api.Application.Repository;
using ZM.DriverService.Api.Application.UnitOfWork;
using ZM.DriverService.Api.Application.UseCases.RegisterDriver;
using ZM.DriverService.Api.Domain.Entities;
using ZM.DriverService.Api.Domain.Enums;
using ZM.RideService.Api.UnitTests;

namespace ZM.DriverService.Api.UnitTests.UseCases
{
    public class RegisterDriverCommandHandlerTests
    {
        [Theory]
        [AutoMoqInlineData]
        public async Task Handle_ValidRequest_RegistersDriver(
            [Frozen] Mock<IDriverRepository> driverRepositoryMock,
            [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            RegisterDriverCommand command,
            RegisterDriverCommandHandler handler)
        {
            // Arrange
            var createdAtUtc = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

            dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(createdAtUtc);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccessful.Should().BeTrue();

            driverRepositoryMock.Verify(
                x => x.CreateDriverAsync(
                    It.Is<Driver>(driver =>
                        driver.FirstName == command.FirstName &&
                        driver.LastName == command.LastName &&
                        driver.Email == command.Email &&
                        driver.PhoneNumber == command.PhoneNumber &&
                        driver.Status == DriverStatus.Offline &&
                        driver.CurrentRideId == null &&
                        driver.CurrentLocation == null &&
                        driver.CreatedAtUtc == createdAtUtc &&
                        driver.Id != Guid.Empty),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}