#nullable disable
using MassTransit;

namespace ZM.RideService.Api.Infrastructure.Sagas.RideCreated
{
    public class RideCreatedSagaData : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public Guid RideId { get; set; }
        public Guid DriverId { get; set; }
        public string RecipientEmail { get; set; }
    }
}