using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ZM.DriverService.Api.Presentation
{
    public class DriverModule : ICarterModule
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
        }
    }
}
