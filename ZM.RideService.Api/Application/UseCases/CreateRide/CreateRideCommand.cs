using MediatR;
using ZM.RideService.Api.Domain.OperationResult;
using ZM.RideService.Api.Domain.ValueObjects;

namespace ZM.RideService.Api.Application.UseCases.CreateRide
{
    public record CreateRideCommand(RiderInfo Rider, Guid? DriverId, RideLocation PickupLocation, RideLocation DestinationLocation) : IRequest<Result>;
}
