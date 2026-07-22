#nullable disable
using MassTransit;
using ZM.RideSharingSystem.Contracts.Commands.Matching;
using ZM.RideSharingSystem.Contracts.Commands.Notification;
using ZM.RideSharingSystem.Contracts.Commands.Payment;
using ZM.RideSharingSystem.Contracts.Commands.Ride;
using ZM.RideSharingSystem.Contracts.Events;

namespace ZM.RideService.Api.Infrastructure.Sagas.RideCreated
{
    public class RideCreatedSaga : MassTransitStateMachine<RideCreatedSagaData>
    {
        public State AwaitingDriverMatch { get; private set; }

        public State AwaitingDriverAssignment { get; private set; }

        public State AwaitingRideCompletion { get; private set; }

        public State AwaitingPayment { get; private set; }

        public Event<RideCreatedEvent> RideCreated { get; private set; }

        public Event<DriverMatchedEvent> DriverMatched { get; private set; }

        public Event<DriverAssignedEvent> DriverAssigned { get; private set; }

        public Event<RideCompletedEvent> RideCompleted { get; private set; }

        public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; }

        public RideCreatedSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => RideCreated,
                x => x.CorrelateById(m => m.Message.RideId));

            Event(() => DriverMatched,
                x => x.CorrelateById(m => m.Message.RideId));

            Event(() => DriverAssigned,
                x => x.CorrelateById(m => m.Message.RideId));

            Event(() => RideCompleted,
                x => x.CorrelateById(m => m.Message.RideId));

            Event(() => PaymentCompleted,
                x => x.CorrelateById(m => m.Message.RideId));

            Initially(
                When(RideCreated)
                    .Then(context =>
                    {
                        context.Saga.RideId = context.Message.RideId;
                        context.Saga.RecipientEmail = context.Message.RecipientEmail;
                    })
                    .TransitionTo(AwaitingDriverMatch)
                    .Publish(context =>
                        new FindDriverCommand(
                            context.Message.RideId,
                            context.Message.Latitude,
                            context.Message.Longitude)));

            During(AwaitingDriverMatch,
                When(DriverMatched)
                    .TransitionTo(AwaitingDriverAssignment)
                    .Publish(context =>
                        new AssignDriverToRideCommand(
                            context.Message.RideId,
                            context.Message.DriverId)));

            During(AwaitingDriverAssignment,
                When(DriverAssigned)
                    .TransitionTo(AwaitingRideCompletion)
                    .Publish(context =>
                        new DriverAssignedNotificationCommand(
                            context.Message.RideId,
                            context.Message.DriverId,
                            context.Saga.RecipientEmail)));

            During(AwaitingRideCompletion,
                When(RideCompleted)
                    .TransitionTo(AwaitingPayment)
                    .Publish(context =>
                        new RideCompletedNotificationCommand(
                            context.Message.RideId,
                            context.Saga.RecipientEmail))
                    .Publish(context =>
                        new ProcessPaymentCommand(context.Message.RideId)));

            During(AwaitingPayment,
                When(PaymentCompleted)
                    .Publish(context =>
                        new PaymentReceiptNotificationCommand(
                            context.Message.PaymentId,
                            context.Message.RideId,
                            context.Message.Amount,
                            context.Saga.RecipientEmail))
                    .Finalize());

            SetCompletedWhenFinalized();
        }
    }
}