# Architecture

How the system is put together, and the file behind each pattern.

- [Service boundaries](#service-boundaries)
- [Layering inside a service](#layering-inside-a-service)
- [Patterns](#patterns)
- [Data and state](#data-and-state)
- [Cross-cutting concerns](#cross-cutting-concerns)

---

## Service boundaries

Five services, split along capability lines rather than along entities:

| Service | Owns | Why it is separate |
|---|---|---|
| **Ride** | The ride aggregate and the workflow itself | The workflow's coordinator; the only service that needs the whole picture |
| **Driver** | Driver identity, status, position | Write-heavy — every driver emits location pings independently of ride volume |
| **Matching** | The searchable index of available drivers | Read-heavy and latency-sensitive; backed by Redis rather than a relational store, and stateless enough to scale horizontally |
| **Payment** | Payment records | Settlement is a distinct concern with its own consistency requirements |
| **Notification** | Delivery to email / SMS / push | Purely reactive and failure-isolated; a notification outage must not stall rides |

Services never call each other over HTTP. Every cross-service interaction is a message, which means a downstream service being unavailable delays work rather than failing the caller.

The full inventory of messages is in [messaging.md](messaging.md).

---

## Layering inside a service

All five services follow the same structure, with dependencies pointing inward:

```
Presentation/     Carter modules — HTTP in, MediatR out
      │
Application/      UseCases/<Feature>/{Command, CommandHandler, DomainEventHandler}
      │           plus the abstractions Infrastructure implements:
      │           IRepository, IUnitOfWork, IDateTimeProvider, IDomainEventCollector
      │
Domain/           Entities, ValueObjects, Enums, Events, Primitives,
      │           OperationResult, ErrorMessages  ← depends on nothing
      │
Infrastructure/   Consumers, Extensions (DI), Sagas, BackgroundJobs,
                  Pricing, Caches, Email/Sms/Push
Persistence/      DbContext, Configurations, Models, Repositories,
                  UnitOfWorks, Queries, Outbox, Idempotence
```

`Application/` declares interfaces; `Infrastructure/` and `Persistence/` implement them. `Domain/` has no outbound dependencies at all — the aggregates reference only their own value objects, events and `Result` type.

The Notification service uses `Common/` where the others use `Domain/`, since it has no aggregate to model — only shared primitives.

---

## Patterns

### Aggregates and domain events

`AggregateRoot` ([ZM.RideService.Api/Domain/Primitives/AggregateRoot.cs](../ZM.RideService.Api/Domain/Primitives/AggregateRoot.cs)) holds a private event list and exposes exactly three members:

```csharp
public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.ToList();
public void ClearDomainEvents() => _domainEvents.Clear();
protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
```

`Raise` is `protected`, so only the aggregate can announce its own changes. Handlers cannot fabricate a domain event on the aggregate's behalf.

Aggregates keep private constructors and expose two static factories:

- **`Create(...)`** — a genuinely new instance. Sets the opening status and raises the creation event.
- **`Rehydrate(...)`** — reconstruction from storage. Takes every field including timestamps, and raises nothing, because reading a row from the database is not a business event.

Every state transition is a method that guards its precondition first and returns `Result`:

```csharp
public Result StartRide(DateTime startedAtUtc)
{
    if (Status != RideStatus.DriverAssigned)
    {
        return Result.Failure(Errors.Ride.DriverIsNotAssigned());
    }

    Status = RideStatus.InProgress;
    StartedAtUtc = startedAtUtc;
    Raise(new RideStartedDomainEvent(Id));

    return Result.Success();
}
```

All setters are `private set`, so the only route to a state change is a method that has already checked the invariant.

See [ZM.RideService.Api/Domain/Entities/Ride.cs](../ZM.RideService.Api/Domain/Entities/Ride.cs), [ZM.DriverService.Api/Domain/Entities/Driver.cs](../ZM.DriverService.Api/Domain/Entities/Driver.cs) and [ZM.PaymentService.Api/Domain/Entities/Payment.cs](../ZM.PaymentService.Api/Domain/Entities/Payment.cs).

### Domain models separated from persistence models

Two representations of the same concept, deliberately:

| Domain — `Domain/Entities/Ride.cs` | Persistence — `Persistence/Models/Ride.cs` |
|---|---|
| `RiderInfo Rider` (value object) | `RiderId`, `RiderFirstName`, `RiderLastName`, `RiderEmail` |
| `RideLocation PickupLocation` | `PickupLatitude`, `PickupLongitude`, `PickupAddress` |
| `private set` throughout | Plain mutable properties |
| Behaviour and invariants | No behaviour |

Repositories translate between them, calling `Rehydrate` on the way in. The payoff is that storage shape and domain shape evolve independently: flattening a value object into columns never forces the aggregate to expose a setter it does not want.

### CQRS-style use cases

One folder per feature under `Application/UseCases/`, containing the command record, its handler, and any domain-event handler that reacts to it.

Writes go through MediatR:

```csharp
public record AssignDriverCommand(Guid RideId, Guid DriverId) : IRequest<Result>;
```

Reads skip the aggregate entirely. `IGetRidesQuery` / `GetRidesQuery` project storage rows straight to `GetRidesDto`, because rebuilding an aggregate only to flatten it again for a list view buys nothing.

### Result and Error

Business rule violations are return values, not exceptions. `Result` ([ZM.RideService.Api/Domain/OperationResult/Result.cs](../ZM.RideService.Api/Domain/OperationResult/Result.cs)) carries `IsSuccessful`, `Message` and `Error`; `Result<T>` adds `Data`. Exceptions stay for genuinely exceptional conditions.

Each service owns a static `Errors` catalogue of factory methods returning `Error(ErrorCode, ErrorMessage)`:

| Code | Meaning |
|---|---|
| `RIDE0001` | Ride not found |
| `RIDE0002` | Driver could not be assigned — ride is not in the requested status |
| `RIDE0003` | Ride could not be started — no driver assigned |
| `RIDE0004` | Ride could not be completed — not in progress |
| `RIDE0005` | Ride could not be cancelled — already completed |
| `DRIVER0001` | Driver not found |
| `DRIVER0002` | Driver is currently in a ride |
| `DRIVER0003` | Driver must be available to be assigned |
| `DRIVER0004` | Driver must be assigned to start a ride |
| `DRIVER0005` | Driver must be in a ride to complete it |
| `PAY0001` | Payment not found |
| `PAY0002` | Payment already completed / payment failed |
| `PAY0003` | Payment could not be processed |
| `MATCHING0001`–`MATCHING0003` | Matching catalogue |
| `NOTIF001`–`NOTIF003` | Notification catalogue |

Codes are stable identifiers a client can branch on; the message is for humans.

### Transactional outbox

The Ride service does not publish domain events directly. It writes them to an outbox table in the same transaction as the aggregate, so an event can never be published for a change that failed to persist — nor lost for one that succeeded.

`OutboxMessage` ([ZM.RideService.Api/Persistence/Outbox/OutboxMessage.cs](../ZM.RideService.Api/Persistence/Outbox/OutboxMessage.cs)):

| Column | Purpose |
|---|---|
| `Id` | Primary key |
| `Type` | `AssemblyQualifiedName` of the event |
| `Content` | Newtonsoft JSON, `TypeNameHandling.All` |
| `OccurredOnUtc` | When it was raised |
| `ProcessedOnUtc` | `null` until dispatched — the work queue |
| `Error` | Last failure, if dispatch did not succeed |

The write path:

1. An aggregate method calls `Raise(...)`.
2. The repository hands those events to `IDomainEventCollector` — a scoped, per-request accumulator ([Persistence/DomainEventCollector/DomainEventCollector.cs](../ZM.RideService.Api/Persistence/DomainEventCollector/DomainEventCollector.cs)).
3. `UnitOfWork.SaveChangesAsync` calls `ConvertDomainEventsToOutboxMessages()` **before** `SaveChangesAsync`, so rows and events land in one save ([Persistence/UnitOfWorks/UnitOfWork.cs](../ZM.RideService.Api/Persistence/UnitOfWorks/UnitOfWork.cs)).

The read path is `ProcessOutboxMessagesJob` ([Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs](../ZM.RideService.Api/Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs)) — Quartz, `[DisallowConcurrentExecution]`, every 5 seconds:

- Takes up to 20 rows where `ProcessedOnUtc == null`.
- Deserializes each back to `IDomainEvent`, using `ConstructorHandling.AllowNonPublicDefaultConstructor` so events with private constructors round-trip.
- Publishes through a Polly pipeline — 3 retries, constant 50 ms backoff.
- Stamps `ProcessedOnUtc` on success, or records `Error` and leaves the row unprocessed.

The Driver service takes the simpler route for comparison: its `UnitOfWork` saves, then publishes collected events directly through MediatR's `IPublisher`. Fewer moving parts, no store-and-forward guarantee — the trade-off is explicit, and the two implementations sit side by side in the codebase.

### Idempotent consumers

Message brokers deliver at least once, so consumers must tolerate replay. Each service keeps a `ProcessedMessage` table:

```csharp
public sealed class ProcessedMessage
{
    public Guid? MessageId { get; set; }
    public string ConsumerName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

`ProcessedMessageConfiguration` gives it a composite key of `{MessageId, ConsumerName}`, caps `ConsumerName` at 200 characters, and adds a unique index. Keying on the pair rather than the message id alone means several consumers can each process the same message once — which is what fan-out requires.

Consumers check for the marker, skip if present, and insert it as part of their work.

### Saga orchestration

The ride workflow spans four services and outlives any single request, so it is modelled explicitly as a MassTransit state machine rather than left implicit in a chain of consumers. `RideCreatedSaga` ([ZM.RideService.Api/Infrastructure/Sagas/RideCreated/RideCreatedSaga.cs](../ZM.RideService.Api/Infrastructure/Sagas/RideCreated/RideCreatedSaga.cs)) declares five states and six correlated events, and every transition is one readable `When(...).TransitionTo(...).Publish(...)` clause.

The whole flow is walked through in **[ride-lifecycle.md](ride-lifecycle.md)**.

### Convention-based dependency injection

Each service exposes one entry point — `RegisterServices(this IServiceCollection, IConfiguration)` in `Infrastructure/Extensions/IServiceCollectionExtensions.cs` — composed of small private helpers, so `Program.cs` stays at a dozen lines:

```csharp
public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
{
    RegisterEntityFramework(services);
    RegisterMediatR(services);
    RegisterMassTransit(services, configuration);
    RegisterQuartz(services);
    RegisterCarter(services);
    RegisterDomainEventCollector(services);
    RegisterPricingServices(services);
    RegisterUnitOfWorks(services);
    RegisterDateTimeProviders(services);
    RegisterQueries(services);
    RegisterRepositories(services);
}
```

The last four scan by type-name suffix with Scrutor instead of listing registrations:

```csharp
services.Scan(scan => scan
    .FromAssemblyOf<RideRepository>()
    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository")))
    .AsMatchingInterface()
    .WithScopedLifetime());
```

`AsMatchingInterface()` binds `RideRepository` to `IRideRepository` by naming convention. Adding a repository, query, unit of work, date-time provider, template set or sender needs no DI edit — the suffix is the registration. Scanned suffixes: `UnitOfWork`, `DateTimeProvider`, `Query`, `Repository`, and in the Notification service also `Templates` and `Sender`.

### Testable time

No aggregate or handler calls `DateTime.UtcNow`. Time arrives as a parameter, supplied by an injected `IDateTimeProvider`:

```csharp
var ride = Ride.Create(Guid.NewGuid(), request.Rider, request.PickupLocation,
                       request.DestinationLocation, estimatedFare, _dateTimeProvider.UtcNow);
```

Tests pin the clock to a fixed instant, which makes timestamp assertions exact rather than approximate.

### Service autonomy over reuse

`Result`, `Error`, `AggregateRoot`, `IDomainEvent`, `IDateTimeProvider` and `ProcessedMessage` are duplicated in every service rather than extracted to a shared library.

This is intentional. A shared kernel of primitives is a coupling point: changing `Result` would mean recompiling and redeploying five services together, which is exactly what service boundaries exist to avoid. The duplication is a few dozen lines per service and buys independent evolution.

The one thing genuinely shared is `ZM.RideSharingSystem.Contracts` — the messages services exchange. That assembly has no package references and contains nothing but records, so depending on it commits a service to a message shape and nothing else.

---

## Data and state

Database per service, with no shared schema and no cross-service joins:

| Service | Context | Sets |
|---|---|---|
| Ride | `RideDbContext` | `Rides`, `OutboxMessages`, `ProcessedMessages` |
| Driver | `DriverDbContext` | `Drivers`, `ProcessedMessages` |
| Payment | `PaymentDbContext` | `Payments`, `ProcessedMessages` |
| Notification | `NotificationDbContext` | `ProcessedMessages` |
| Matching | — | Redis only |

Every context registers the EF Core **InMemory provider** with `QueryTrackingBehavior.TrackAll`, and saga state uses MassTransit's `InMemoryRepository()`. Storage is therefore process-lifetime: the system starts from empty on every run, which suits a reference implementation where the interesting behaviour is the message flow rather than the durability story. Swapping to a durable provider is a change to `RegisterEntityFramework` and the saga repository registration; the aggregates and repositories are already isolated from it.

Entity configuration uses `IEntityTypeConfiguration<T>` classes applied through `ApplyConfigurationsFromAssembly`, keeping mapping out of `OnModelCreating`.

---

## Cross-cutting concerns

**Pipeline.** Each service's `Program.cs` is the same shape:

```csharp
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapCarter();   // omitted in Matching and Notification
app.Run();
```

**Message bus.** Configured identically everywhere — `SetKebabCaseEndpointNameFormatter()`, a RabbitMQ host from `MessageBroker:Host|Username|Password`, and `ConfigureEndpoints(context)` to bind registered consumers to conventionally named queues.

**Access control.** The services carry no authentication or authorization layer. Riders are identified by the `RiderInfo` supplied in the request body and drivers by a `Guid`; both APIs are anonymous. An edge gateway or per-service authentication middleware is where that would be introduced.

**Configuration.** Environment-specific values come from `appsettings.json` and environment variables. Compose sets `ASPNETCORE_URLS` per service, and each project declares a `UserSecretsId` for local secret storage.
