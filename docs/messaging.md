# Messaging

Every cross-service interaction in the system. Services never call each other over HTTP — the message contracts below *are* the integration surface.

- [Bus configuration](#bus-configuration)
- [The contracts assembly](#the-contracts-assembly)
- [Commands](#commands)
- [Events](#events)
- [Consumers](#consumers)
- [Domain events vs. integration events](#domain-events-vs-integration-events)
- [Idempotency and delivery](#idempotency-and-delivery)

---

## Bus configuration

Identical in all five services, in `Infrastructure/Extensions/IServiceCollectionExtensions.cs`:

```csharp
services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    busConfigurator.AddConsumer<AssignDriverConsumer>();
    // ...

    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(configuration["MessageBroker:Host"]!), h =>
        {
            h.Username(configuration["MessageBroker:Username"]!);
            h.Password(configuration["MessageBroker:Password"]!);
        });

        configurator.ConfigureEndpoints(context);
    });
});
```

`SetKebabCaseEndpointNameFormatter()` turns `AssignDriverConsumer` into the queue `assign-driver`, and `ConfigureEndpoints(context)` binds every registered consumer to its conventional queue — so registering a consumer is the whole of the wiring. The queues are visible in the RabbitMQ management UI at http://localhost:15672.

The Ride service additionally registers the saga:

```csharp
busConfigurator.AddSagaStateMachine<RideCreatedSaga, RideCreatedSagaData>()
               .InMemoryRepository();
```

---

## The contracts assembly

`ZM.RideSharingSystem.Contracts` is the only assembly more than one service references. It is deliberately minimal:

- **No package references at all** — nothing but `net10.0`. A service that depends on it inherits no transitive dependencies and no version conflicts.
- **Nothing but `record` declarations** — no behaviour, no validation, no base classes. Each file is three lines.
- **Positional records**, so a message is immutable and value-compared by default.

Keeping it this thin is what makes it safe to share. Anything richer — a base class, a helper, a validator — would become a coupling point that forces services to redeploy together.

Adding a field to an existing record is a breaking change for consumers compiled against the old shape. New optional information belongs in a new message rather than an extra positional parameter.

---

## Commands

A command names a specific action for one service to perform. All eight live under `ZM.RideSharingSystem.Contracts/Commands/`.

| Command | Signature | Published by | Consumed by |
|---|---|---|---|
| `FindDriverCommand` | `(Guid RideId, double Latitude, double Longitude)` | Saga | Matching — `FindDriverConsumer` |
| `AssignDriverToRideCommand` | `(Guid RideId, Guid DriverId)` | Saga | Ride — `AssignDriverConsumer` |
| `AssignRideToDriverCommand` | `(Guid RideId, Guid DriverId)` | Saga | Driver — `AssignRideToDriverConsumer` |
| `CompleteRideCommand` | `(Guid RideId)` | — | Ride — `CompleteRideConsumer` |
| `ProcessPaymentCommand` | `(Guid RideId)` | Saga | Payment |
| `DriverAssignedNotificationCommand` | `(Guid RideId, Guid DriverId, string RecipientEmail)` | Saga | Notification — `DriverAssignedConsumer` |
| `RideCompletedNotificationCommand` | `(Guid RideId, string RecipientEmail)` | Saga | Notification — `RideCompletedConsumer` |
| `PaymentReceiptNotificationCommand` | `(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail)` | Saga | Notification — `PaymentReceiptConsumer` |

Notification commands carry `RecipientEmail` rather than a rider id. The Notification service therefore needs no ability to look up riders — it has no database of its own beyond an idempotency table, and adding a channel never means adding a dependency on the Ride service. The saga captures the address once, from `RideCreatedEvent`, and carries it in saga state for the life of the workflow.

`CompleteRideCommand` gives the Ride service a message-driven route into completion alongside the HTTP endpoint, so ride completion could later be triggered by another service — a driver-side app signalling arrival, say — without changing the Ride service.

---

## Events

An event announces that something happened. The publisher does not know or care who listens. All 13 live under `ZM.RideSharingSystem.Contracts/Events/`.

### Ride workflow

| Event | Signature | Published by | Consumed by |
|---|---|---|---|
| `RideCreatedEvent` | `(Guid RideId, double Latitude, double Longitude, string RecipientEmail)` | Ride — `RideCreatedDomainEventHandler` | Saga (starts instance) |
| `DriverMatchedEvent` | `(Guid RideId, Guid DriverId)` | Matching — `FindDriverCommandHandler` | Saga |
| `DriverNotFoundEvent` | `(Guid RideId)` | Matching — `FindDriverCommandHandler` | — |
| `DriverAssignedEvent` | `(Guid RideId, Guid DriverId)` | Driver — `RideAssignedDomainEventHandler` | Saga |
| `RideStartedEvent` | `(Guid RideId)` | Ride — `RideStartedDomainEventHandler` | Saga, Ride — `RideStartedConsumer` |
| `RideCompletedEvent` | `(Guid RideId)` | Ride — `RideCompleteDomainEventHandler` | Saga |
| `PaymentCompletedEvent` | `(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail)` | Payment — `PaymentCompletedDomainEventHandler` | Saga |

`DriverNotFoundEvent` is published whenever no driver is within range, giving the workflow an explicit "no match" signal to react to rather than leaving the absence of a match indistinguishable from a slow one.

### Driver signals

| Event | Signature | Published by | Consumed by |
|---|---|---|---|
| `DriverAvailableEvent` | `(Guid DriverId)` | Driver — `DriverAvailableDomainEventHandler` | Matching — `DriverAvailableConsumer` |
| `DriverUnavailableEvent` | `(Guid DriverId)` | Driver — `DriverUnavailableDomainEventHandler` | Matching — `DriverUnavailableConsumer` |
| `DriverLocationUpdatedEvent` | `(Guid DriverId, double Latitude, double Longitude)` | Driver — `DriverLocationUpdatedDomainEventHandler` | Matching — `DriverLocationUpdatedConsumer` |
| `DriverStartRideEvent` | `(Guid DriverId)` | Saga | Driver — `DriverStartRideConsumer` |
| `DriverCompleteRideEvent` | `(Guid DriverId)` | Saga | Driver — `DriverCompleteRideConsumer` |

These three driver signals are what keep the Redis matching index current. The Driver service owns driver state and simply announces changes; Matching decides what to do with them. Neither service knows anything about the other.

| Event | Signature | Notes |
|---|---|---|
| `PaymentProcessedNotificationSentEvent` | `()` | Declared as a receipt-delivery signal; no publisher or consumer wired up |

---

## Consumers

Fourteen consumers across four services. Matching and Notification are reached *only* this way.

### Ride Service — `ZM.RideService.Api/Infrastructure/Consumers/`

| Consumer | Consumes | Sends internally |
|---|---|---|
| `AssignDriverConsumer` | `AssignDriverToRideCommand` | `AssignDriverCommand` |
| `RideStartedConsumer` | `RideStartedEvent` | `StartRideCommand` |
| `CompleteRideConsumer` | `CompleteRideCommand` | `CompleteRideCommand` (internal) |

### Driver Service — `ZM.DriverService.Api/Infrastructure/Consumers/`

| Consumer | Consumes | Effect |
|---|---|---|
| `AssignRideToDriverConsumer` | `AssignRideToDriverCommand` | `Driver.AssignRide(...)` → `Assigned` |
| `DriverStartRideConsumer` | `DriverStartRideEvent` | `Driver.StartRide(...)` → `InRide` |
| `DriverCompleteRideConsumer` | `DriverCompleteRideEvent` | `Driver.CompleteRide(...)` → `Available` |

### Matching Service — `ZM.MatchingService.Api/Infrastructure/Consumers/`

| Consumer | Consumes | Effect on Redis |
|---|---|---|
| `DriverAvailableConsumer` | `DriverAvailableEvent` | Adds to `drivers:available` |
| `DriverUnavailableConsumer` | `DriverUnavailableEvent` | Removes from set, geo index and metadata |
| `DriverLocationUpdatedConsumer` | `DriverLocationUpdatedEvent` | Updates `drivers:geo` position |
| `FindDriverConsumer` | `FindDriverCommand` | Runs the radius search |

### Payment Service

| Consumer | Consumes |
|---|---|
| `ProcessPaymentConsumer` | `ProcessPaymentCommand` |

### Notification Service — `ZM.NotificationService.Api/Infrastructure/Consumers/`

| Consumer | Consumes | Template |
|---|---|---|
| `DriverAssignedConsumer` | `DriverAssignedNotificationCommand` | `EmailTemplates.Driver.Assigned` |
| `RideCompletedConsumer` | `RideCompletedNotificationCommand` | `EmailTemplates.Ride.Completed` |
| `PaymentReceiptConsumer` | `PaymentReceiptNotificationCommand` | `EmailTemplates.Payment.Receipt` |

### The consumer's job

Consumers are thin. They check idempotency, translate the transport message into an internal MediatR request, and delegate — no business logic:

```csharp
public async Task Consume(ConsumeContext<AssignDriverToRideCommand> context)
{
    if (await _dbContext.ProcessedMessages.AnyAsync(p =>
            p.MessageId == context.MessageId &&
            p.ConsumerName == nameof(AssignDriverConsumer),
            context.CancellationToken))
    {
        return;
    }

    try
    {
        await _dbContext.ProcessedMessages.AddAsync(new ProcessedMessage { ... });

        await _sender.Send(new AssignDriverCommand(
            context.Message.RideId,
            context.Message.DriverId), context.CancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error processing {nameof(AssignDriverConsumer)} ...");
    }
}
```

The translation step matters: a public `AssignDriverToRideCommand` becomes an internal `AssignDriverCommand`. The two shapes are free to diverge, and the same use-case handler serves both the message path and the HTTP endpoint. `FindDriverConsumer` shows the richer form of this, re-wrapping loose coordinates into a `GeoLocation` value object on the way in.

Where a consumer's message type shares its name with an internal command, the consumer qualifies the internal one explicitly, since the message type it binds to determines its queue:

```csharp
await _sender.Send(new Application.UseCases.FindDriver.FindDriverCommand(...));
```

---

## Domain events vs. integration events

Two distinct kinds of event, and the distinction runs through the whole codebase:

| | Domain event | Integration event |
|---|---|---|
| Example | `RideCreatedDomainEvent` | `RideCreatedEvent` |
| Lives in | `Domain/Events/` of one service | `ZM.RideSharingSystem.Contracts` |
| Scope | In-process, via MediatR | Across services, via RabbitMQ |
| Raised by | The aggregate, through `Raise(...)` | A domain-event handler |
| Free to change | Yes — internal | No — a published contract |

A domain-event handler is the bridge, and it is where a service decides what to expose:

```csharp
public async Task Handle(RideCreatedDomainEvent notification, CancellationToken cancellationToken)
{
    await _bus.Publish(new RideCreatedEvent(
        notification.RideId,
        notification.Latitude,
        notification.Longitude,
        notification.Rider.Email), cancellationToken);
}
```

`RideCreatedDomainEvent` carries the full `RiderInfo`; `RideCreatedEvent` publishes only the e-mail address. Internal richness stays internal, and the published contract stays as narrow as its consumers actually need.

---

## Idempotency and delivery

RabbitMQ delivers at least once, so a consumer may see the same message twice — on a broker redelivery, or after a consumer crashes mid-work.

Each service therefore keeps a `ProcessedMessage` table keyed on `{MessageId, ConsumerName}`. Keying on the pair is what makes fan-out work: three Notification consumers can each process the same broadcast exactly once, which a message-id-only key would prevent.

Consumers catch their exceptions and log them via `ILogger` rather than rethrowing. A failed message is recorded and the consumer moves on, so one bad message cannot block the queue behind it. Escalating to MassTransit's retry and dead-letter facilities — `UseMessageRetry`, `_error` queues — is a change to the bus configuration in `RegisterMassTransit`, and would not touch the consumers themselves.

On the publishing side, the Ride service does not publish inline at all: domain events go to an outbox table in the same transaction as the aggregate and are dispatched by a background job. See [architecture.md](architecture.md#transactional-outbox).
