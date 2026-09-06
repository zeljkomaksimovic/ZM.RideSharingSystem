# Notification Service

`ZM.NotificationService.Api` — port **5400** (local: 5201 / 7289)

Turns workflow events into messages a rider receives. Like the Matching service it is message-only, but for the opposite reason: it is a pure sink, the last stop for three of the saga's transitions.

- [Shape of the service](#shape-of-the-service)
- [Use cases](#use-cases)
- [Consumers](#consumers)
- [Templates](#templates)
- [Delivery channels](#delivery-channels)
- [Persistence](#persistence)
- [Configuration](#configuration)
- [Tests](#tests)

---

## Shape of the service

No aggregate, no domain entities, no HTTP endpoints. `Program.cs` registers the bus and Swagger and runs — no `MapCarter()`, because there is nothing to expose.

That absence is the design. Notification has no state worth owning: a notification is sent once and is then history. The service exists to isolate a failure domain — an unreachable e-mail provider must never stall a ride — and to keep delivery concerns out of the services that generate the events.

For the same reason it uses `Common/` where the other services use `Domain/`. It holds the shared primitives (`Result`, `Error`) but nothing that would earn the name *domain*, and naming the folder honestly says so.

Every message it consumes carries `RecipientEmail`, so it never looks a rider up. It has no dependency on the Ride service, no rider table, and no reason to acquire either.

---

## Use cases

`Application/UseCases/`

| Use case | Handler | Template |
|---|---|---|
| SendDriverAssignedNotification | `SendDriverAssignedNotificationCommandHandler` | `Driver.Assigned` |
| SendRideCompletedNotification | `SendRideCompletedNotificationCommandHandler` | `Ride.Completed` |
| SendPaymentReceiptNotification | `SendPaymentReceiptNotificationCommandHandler` | `Payment.Receipt` |
| SendRideCancelledNotification | `SendRideCancelledNotificationCommandHandler` | `Ride.Cancelled` |

Each handler resolves a template, renders it, and hands the result to `IEmailSender`. No branching on message type, no formatting inline — the template owns the wording, the handler owns the dispatch.

`SendRideCancelledNotificationCommandHandler` is implemented and unit-tested, and pairs with the Ride service's `CancelRideCommandHandler`. Both halves of ride cancellation exist as use cases; wiring them to the bus is a matter of adding a cancellation contract and a consumer, with no change to either handler.

---

## Consumers

`Infrastructure/Consumers/`

| Consumer | Consumes | Sends internally |
|---|---|---|
| `DriverAssignedConsumer` | `DriverAssignedNotificationCommand` | `SendDriverAssignedNotificationCommand` |
| `RideCompletedConsumer` | `RideCompletedNotificationCommand` | `SendRideCompletedNotificationCommand` |
| `PaymentReceiptConsumer` | `PaymentReceiptNotificationCommand` | `SendPaymentReceiptNotificationCommand` |

These three correspond to the saga's three notification points: driver assigned, ride completed, receipt issued.

They are also the strictest about idempotency in the codebase — the only consumers that explicitly `SaveChangesAsync` their `ProcessedMessage` marker rather than relying on a downstream save. They have no other persistence work to piggyback on, and the consequence of a missed marker here is visible to the customer: a duplicate e-mail.

---

## Templates

[Application/Templates/EmailTemplates.cs](../../ZM.NotificationService.Api/Application/Templates/EmailTemplates.cs) — static nested classes, one per subject area, each pairing a subject constant with a body function:

| Template | Subject | Body parameters |
|---|---|---|
| `Driver.Assigned` | "Driver Assigned" | `rideId`, `driverId` |
| `Payment.Receipt` | "Payment Receipt" | `paymentId`, `rideId`, `amount` |
| `Ride.Completed` | "Ride Completed" | `rideId` |
| `Ride.Cancelled` | "Ride Cancelled" | `rideId` |

Bodies are C# raw string literals, so the template reads in the file as it will read in the inbox:

```csharp
public static string Receipt(Guid paymentId, Guid rideId, decimal amount)
{
    return
        $"""
        Dear Customer,

        Your payment has been successfully processed.

        Payment Id: {paymentId}
        Ride Id: {rideId}
        Amount: {amount:C}

        """;
}
```

`{amount:C}` uses the standard currency format, so the rendered symbol follows the host's culture.

Being functions rather than format strings means the compiler checks every field a template needs. Adding a parameter breaks the call site instead of silently rendering a gap.

---

## Delivery channels

Three senders, each behind an interface in `Application/NotificationSender/`:

| Interface | Implementation | Location |
|---|---|---|
| `IEmailSender` | `EmailSender` | `Infrastructure/Email/` |
| `ISmsSender` | `SmsSender` | `Infrastructure/Sms/` |
| `IPushNotificationSender` | `PushNotificationSender` | `Infrastructure/Push/` |

All three are logging stand-ins. `EmailSender` says so directly:

```csharp
//Note: This is a mock implementation. In a real-world scenario, you would integrate with an email service provider.
public Task SendEmailAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
{
    _logger.LogInformation("Sending email to {Recipient}. Subject: {Subject}. Body: {Body}", recipient, subject, body);
    return Task.CompletedTask;
}
```

This makes the whole notification path observable without any external account: `docker compose logs -f zm.notificationservice.api` shows the exact messages a rider would receive, rendered.

The interfaces are the integration seam. Swapping in SendGrid, Twilio or Firebase means replacing one class each — Scrutor picks the new implementation up automatically from the `Sender` suffix, so even the DI registration stays untouched. Handlers, templates, consumers and contracts are all unaffected, because none of them knows how a message is actually delivered.

`ISmsSender` and `IPushNotificationSender` have no callers yet: every current notification is an e-mail. They are registered and ready, so adding an SMS on driver assignment is a change inside one handler.

---

## Persistence

`NotificationDbContext` — `ProcessedMessages` only. EF Core InMemory database `"Notification"`.

The single table is the idempotency ledger. Sent notifications are not stored: the service's job ends when the message is handed to a provider, and a real deployment would read delivery history from the provider's own records rather than duplicating them here.

---

## Configuration

```json
{
  "MessageBroker": {
    "Host": "amqp://distributedsys-mq:5672",
    "Username": "admin",
    "Password": "admin"
  }
}
```

DI: EF, MediatR, MassTransit with three consumers, then Scrutor scans for the `Templates` and `Sender` suffixes alongside the usual ones. No Carter registration.

---

## Tests

`ZM.NotificationService.Api.UnitTests` — one file per handler:

| Test file | Case |
|---|---|
| `SendDriverAssignedNotificationCommandHandlerTests` | Sends the driver-assigned e-mail |
| `SendRideCompletedNotificationCommandHandlerTests` | Sends the ride-completed e-mail |
| `SendPaymentReceiptNotificationCommandHandlerTests` | Sends the payment-receipt e-mail |
| `SendRideCancelledNotificationCommandHandlerTests` | Sends the ride-cancelled e-mail |

Tests mock `IEmailSender` and assert it was called with the expected recipient and subject. There are no `Builders/` here — with no aggregate to construct, the commands are simple enough to build inline.
