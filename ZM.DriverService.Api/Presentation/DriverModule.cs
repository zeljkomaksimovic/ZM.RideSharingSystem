using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZM.DriverService.Api.Application.UseCases.GetDrivers;
using ZM.DriverService.Api.Application.UseCases.RegisterDriver;
using ZM.DriverService.Api.Application.UseCases.SetDriverAvailable;
using ZM.DriverService.Api.Application.UseCases.SetDriverUnavailable;
using ZM.DriverService.Api.Application.UseCases.UpdateDriverLocation;

namespace ZM.DriverService.Api.Presentation
{
    public class DriverModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/GetDrivers", async (ISender sender) =>
            {
                var drivers = await sender.Send(new GetDriversRequest());
                return Results.Ok(drivers);
            });

            app.MapPost("api/RegisterDriver", async ([FromBody] RegisterDriverCommand request, ISender sender) =>
            {
                var driver = await sender.Send(request);
                return Results.Ok(driver);
            });

            app.MapPut("api/SetDriverAvailable", async ([FromBody] SetDriverAvailableCommand request, ISender sender) =>
            {
                var driver = await sender.Send(request);
                return Results.Ok(driver);
            });

            app.MapPut("api/SetDriverUnavailable", async ([FromBody] SetDriverUnavailableCommand request, ISender sender) =>
            {
                var driver = await sender.Send(request);
                return Results.Ok(driver);
            });

            app.MapPut("api/UpdateDriverLocation", async ([FromBody] UpdateDriverLocationCommand request, ISender sender) =>
            {
                var driver = await sender.Send(request);
                return Results.Ok(driver);
            });
        }
    }
}
