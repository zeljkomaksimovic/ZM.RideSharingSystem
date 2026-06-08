using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZM.RideService.Api.Application.UseCases.AssignDriver;
using ZM.RideService.Api.Application.UseCases.CompleteRide;
using ZM.RideService.Api.Application.UseCases.CreateRide;
using ZM.RideService.Api.Application.UseCases.GetRides;
using ZM.RideService.Api.Application.UseCases.StartRide;

namespace ZM.RideService.Api.Presentation
{
    public class RideModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/GetRides", async (ISender sender) =>
            {
                var rides = await sender.Send(new GetRidesRequest());
                return Results.Ok(rides);
            });

            app.MapPost("api/CreateRide", async([FromBody] CreateRideCommand request, ISender sender) => 
            {
                await sender.Send(request);
                return Results.Ok();
            });

            app.MapPut("api/AssignDriver", async ([FromBody] AssignDriverCommand request, ISender sender) =>
            {
                await sender.Send(request);
                return Results.Ok();
            });

            app.MapPut("api/StartRide", async ([FromBody] CancelRideCommand request, ISender sender) =>
            {
                await sender.Send(request);
                return Results.Ok();
            });

            app.MapPut("api/CompleteRide", async ([FromBody] CompleteRideCommand request, ISender sender) =>
            {
                await sender.Send(request);
                return Results.Ok();
            });
        }
    }
}
