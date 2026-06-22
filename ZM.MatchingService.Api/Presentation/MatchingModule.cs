using Carter;
using MediatR;
using ZM.MatchingService.Api.Application.UseCases.GetAvailableDrivers;

namespace ZM.MatchingService.Api.Presentation
{
    public class MatchingModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/GetAvailableDrivers", async (ISender sender) =>
            {
                var drivers = await sender.Send(new GetAvailableDriversRequest());
                return Results.Ok(drivers);
            });
        }
    }
}
