using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZM.RideService.Api.Application.UseCases.CreateRide;

namespace ZM.RideService.Api.Presentation
{
    public class RideModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/CreateRide", async([FromBody] CreateRideCommand request, ISender sender) => 
            {
                await sender.Send(request);
                return Results.Ok();
            });
        }
    }
}
