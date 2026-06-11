#nullable disable
using MassTransit;
using ZM.RideSharingSystem.Contracts.Commands;
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

        public Event<RideCreatedEvent> RideCreated { get; private set; }
        public Event<DriverMatchedEvent> DriverMatched { get; private set; }
        public Event<DriverAssignedEvent> DriverAssigned { get; private set; }
        public Event<RideCompletedEvent> RideCompleted { get; private set; }
        public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; }
        public Event<PaymentProcessedNotificationSentEvent> PaymentProcessedNotificationSent { get; private set; }

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
               .Publish(context => new FindDriverCommand(context.Message.RideId)));

            During(AwaitingDriverMatch,
                When(DriverMatched)
                .Then(context =>
                {
                    context.Saga.DriverMatched = true;
                })
                .TransitionTo(AwaitingDriverAssignment)
                .Publish(context => new AssignDriverCommand(context.Message.RideId, context.Message.DriverId)));

            During(AwaitingDriverAssignment,
                When(DriverAssigned)
                .Then(context =>
                {
                    context.Saga.DriverAssigned = true;
                })
                .TransitionTo(AwaitingRideCompletion));

            During(AwaitingRideCompletion,
                When(RideCompleted)
                .Then(context =>
                {
                    context.Saga.RideCompleted = true;
                })
                .TransitionTo(AwaitingPayment)
                .Publish(context => new ProcessPaymentCommand(context.Message.RideId)));

            During(AwaitingPayment,
               When(PaymentCompleted)
               .Then(context =>
               {
                   context.Saga.PaymentCompleted = true;
               })
               .TransitionTo(AwaitingNotification)
               .Publish(context => new PaymentReceiptNotificationCommand(context.Message.RideId)));

            During(AwaitingNotification,
               When(PaymentProcessedNotificationSent)
               .Then(context =>
               {
                   context.Saga.NotificationSent = true;
               })
               .Finalize());

        }
    }
}
