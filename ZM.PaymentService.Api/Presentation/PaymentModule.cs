using Carter;
using MediatR;
using ZM.PaymentService.Api.Application.UseCases.GetPaymentById;
using ZM.PaymentService.Api.Application.UseCases.GetPaymentByRideId;

namespace ZM.PaymentService.Api.Presentation
{
    public class PaymentModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/GetPaymentById/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetPaymentByIdQuery(id));
                return Results.Ok(result);
            });

            app.MapGet("api/GetPaymentByRideId/{rideId}", async (Guid rideId, ISender sender) =>
            {
                var result = await sender.Send(new GetPaymentByRideIdQuery(rideId));
                return Results.Ok(result);
            });
        }
    }
}
