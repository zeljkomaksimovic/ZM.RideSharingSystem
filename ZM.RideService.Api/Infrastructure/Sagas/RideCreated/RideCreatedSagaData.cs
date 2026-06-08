#nullable disable
using MassTransit;

namespace ZM.RideService.Api.Infrastructure.Sagas.RideCreated
{
    public class RideCreatedSagaData : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public Guid RideId { get; set; }
        public bool DriverMatched { get; set; }
        public bool DriverAssigned { get; set; }
        public bool RideCompleted { get; set; }
        public bool PaymentCompleted { get; set; }
        public bool NotificationSent { get; set; }
    }
}
