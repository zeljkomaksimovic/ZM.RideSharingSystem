using Carter;
using MediatR;
using ZM.DriverService.Api.Application.UseCases.GetDrivers;

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
        }
    }
}
