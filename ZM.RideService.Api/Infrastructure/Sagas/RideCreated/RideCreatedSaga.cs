#nullable disable
using MassTransit;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.RideService.Api.Infrastructure.Sagas.RideCreated
{
    public class RideCreatedSaga : MassTransitStateMachine<RideCreatedSagaData>
    {
        public State AwaitingDriverMatch { get; private set; }
        public State AwaitingDriverAssignment { get; private set; }
        public State AwaitingRideCompletion { get; private set; }
        public State AwaitingPayment { get; private set; }
        public State AwaitingNotification { get; private set; }

        public Event<RideCreatedIntegrationEvent> RideCreated { get; private set; }

        public RideCreatedSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => RideCreated, e => e.CorrelateById(m => m.Message.RideId));

            Initially(
                When(RideCreated)
                .Then(context =>
                {
                    context.Saga.RideId = context.Message.RideId;
                })
                .TransitionTo(AwaitingDriverMatch)
               //TODO .Publish()
                );
        }
    }
}
