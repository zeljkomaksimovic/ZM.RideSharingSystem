# Ride Service

`ZM.RideService.Api` — port **5000** (local: 5028 / 7131)

The orchestrator. It owns the ride aggregate, prices fares, and hosts the saga that drives the whole workflow. It is also the only service using the transactional outbox, making it the richest of the five.

- [Domain model](#domain-model)
- [Endpoints](#endpoints)
- [Use cases](#use-cases)
- [Pricing](#pricing)
- [The outbox](#the-outbox)
- [Saga host](#saga-host)
- [Consumers](#consumers)
- [Persistence](#persistence)
- [Configuration](#configuration)
- [Tests](#tests)

---

## Domain model

### `Ride` aggregate

[Domain/Entities/Ride.cs](../../ZM.RideService.Api/Domain/Entities/Ride.cs)

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | |
| `Rider` | `RiderInfo` | Value object |
| `DriverId` | `Guid?` | Null until assigned |
| `PickupLocation` | `RideLocation` | Value object |
| `DestinationLocation` | `RideLocation` | Value object |
| `Status` | `RideStatus` | |
| `EstimatedFare` | `decimal?` | Quoted at creation |
| `ActualFare` | `decimal?` | Computed at completion |
| `CreatedAtUtc` | `DateTime` | |
| `AssignedAtUtc` | `DateTime?` | |
| `StartedAtUtc` | `DateTime?` | The final fare's duration is measured from here |
| `CompletedAtUtc` | `DateTime?` | |
| `CancelledAtUtc` | `DateTime?` | |

Every setter is `private set`. Two constructors are private, and instances come from `Create(...)` (new ride, raises `RideCreatedDomainEvent`) or `Rehydrate(...)` (from storage, raises nothing).

**Behaviour and guards:**

| Method | Requires | Raises | Failure |
|---|---|---|---|
| `Create` | — | `RideCreatedDomainEvent` | — |
| `AssignDriver` | `Requested` | `DriverAssignedDomainEvent` | `RIDE0002` |
| `StartRide` | `DriverAssigned` | `RideStartedDomainEvent` | `RIDE0003` |
| `CompleteRide` | `InProgress` | `RideCompletedDomainEvent` | `RIDE0004` |
| `CancelRide` | not `Completed` | — | `RIDE0005` |

### Value objects

```csharp
public record RideLocation(double Latitude, double Longitude, string Address);
public record RiderInfo(Guid Id, string FirstName, string LastName, string Email);
```

`RiderInfo` is a snapshot supplied by the caller, not a foreign key — the Ride service holds no rider table and never resolves a rider id against another service.

### `RideStatus`

```csharp
Requested = 1, DriverAssigned = 2, DriverArrived = 3,
InProgress = 4, Completed = 5, Cancelled = 6
```

Explicitly numbered so stored integers stay stable as members are added. `DriverArrived` is a declared slot for the arrival step and is not currently set by any transition.

### Domain events

`Domain/Events/` — all implement `IDomainEvent : INotification`:

| Event | Payload |
|---|---|
| `RideCreatedDomainEvent` | `(Guid RideId, double Latitude, double Longitude, RiderInfo Rider)` |
| `DriverAssignedDomainEvent` | `(Guid RideId, Guid DriverId)` |
| `RideStartedDomainEvent` | `(Guid RideId)` |
| `RideCompletedDomainEvent` | `(Guid RideId, RiderInfo Rider)` |

---

## Endpoints

[Presentation/RideModule.cs](../../ZM.RideService.Api/Presentation/RideModule.cs)

| Verb | Route | Command |
|---|---|---|
| GET | `api/GetRides` | `GetRidesRequest` |
| POST | `api/CreateRide` | `CreateRideCommand(RiderInfo, RideLocation, RideLocation)` |
| PUT | `api/AssignDriver` | `AssignDriverCommand(Guid RideId, Guid DriverId)` |
| PUT | `api/StartRide` | `StartRideCommand(Guid RideId)` |
| PUT | `api/CompleteRide` | `CompleteRideCommand(Guid RideId)` |

Request and response bodies: [api-reference.md](../api-reference.md#ride-service--5000).

---

## Use cases

`Application/UseCases/<Feature>/` — command, handler, and any domain-event handler together.

| Use case | Handler | What it does |
|---|---|---|
| CreateRide | `CreateRideCommandHandler` | Estimates fare → `Ride.Create` → save |
| AssignDriver | `AssignDriverCommandHandler` | Load → `AssignDriver` → save |
| StartRide | `StartRideCommandHandler` | Load → `StartRide` → save |
| CompleteRide | `CompleteRideCommandHandler` | Load → calculate fare → `CompleteRide` → save |
| CancelRide | `CancelRideCommandHandler` | Load → `CancelRide` → save |
| GetRides | `GetRidesRequestHandler` | Delegates to `IGetRidesQuery` |

Handlers follow one shape — load, guard on `null`, invoke the aggregate, return early if the `Result` failed, persist:

```csharp
var ride = await _rideRepository.GetRideByIdAsync(request.RideId, cancellationToken);
if (ride is null)
{
    return Result.Failure(Errors.Ride.RideNotFound());
}

var completedAt = _dateTimeProvider.UtcNow;
var finalFare = await _fareCalculator.CalculateAsync(ride, completedAt, cancellationToken);

var completeRideResult = ride.CompleteRide(finalFare, completedAt);
if (completeRideResult.IsSuccessful is false)
{
    return completeRideResult;
}

await _rideRepository.CompleteRideAsync(ride, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

The handler never decides whether a transition is legal — it asks the aggregate and forwards the answer.

`CancelRideCommandHandler` is implemented and unit-tested. It is reachable in-process via MediatR; adding a route or a consumer for it needs no change to the use case itself.

**Domain-event handlers** translate internal events into published contracts:

| Handler | Publishes |
|---|---|
| `RideCreatedDomainEventHandler` | `RideCreatedEvent` |
| `DriverAssignedDomainEventHandler` | — |
| `RideStartedDomainEventHandler` | `RideStartedEvent` |
| `RideCompleteDomainEventHandler` | `RideCompletedEvent` and `RideCompletedNotificationCommand` |

---

## Pricing

Two calculators, deliberately separate, both under `Infrastructure/Pricing/`.

### `FareEstimator` — the quote

[Infrastructure/Pricing/FareEstimator.cs](../../ZM.RideService.Api/Infrastructure/Pricing/FareEstimator.cs)

```
fare = 2.50 + (distanceKm × 1.20)
```

Runs at ride creation, before a driver exists. Distance only — duration is unknowable at quote time.

### `FareCalculator` — the charge

[Infrastructure/Pricing/FareCalculator.cs](../../ZM.RideService.Api/Infrastructure/Pricing/FareCalculator.cs)

```
fare = 2.50 + (distanceKm × 1.20) + (durationMinutes × 0.30)
duration = completedAtUtc − StartedAtUtc
```

Runs at completion, when the actual duration is known.

Both use the **Haversine formula** over an earth radius of 6371 km, measuring straight-line pickup-to-destination distance, and round to two decimals with `MidpointRounding.AwayFromZero`. There is no routing or map integration — the great-circle distance stands in for road distance.

Separating the two behind `IFareEstimator` and `IFareCalculator` keeps the quote and the charge independently changeable: introducing surge pricing at quote time, or a minimum fare at charge time, touches one implementation.

---

## The outbox

The only service using it. See [architecture.md](../architecture.md#transactional-outbox) for the full pattern.

**Write side** — `Persistence/UnitOfWorks/UnitOfWork.cs`:

```csharp
public Task SaveChangesAsync(CancellationToken cancellationToken = default)
{
    ConvertDomainEventsToOutboxMessages();
    return _dbContext.SaveChangesAsync(cancellationToken);
}
```

Conversion happens *before* the save, so aggregate rows and outbox rows commit together. Events are serialized with Newtonsoft under `TypeNameHandling.All`, and `Type` records the `AssemblyQualifiedName`.

**Read side** — `Infrastructure/BackgroundJobs/ProcessOutboxMessagesJob.cs`, a Quartz job marked `[DisallowConcurrentExecution]`, triggered every 5 seconds:

```csharp
configure.AddJob<ProcessOutboxMessagesJob>(jobKey, job => job.WithIdentity(jobKey))
         .AddTrigger(options => options.ForJob(jobKey)
             .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
```

Each pass takes up to 20 unprocessed rows, deserializes each back to `IDomainEvent` (with `ConstructorHandling.AllowNonPublicDefaultConstructor`, so events with private constructors round-trip), and publishes through a Polly pipeline of 3 retries at a constant 50 ms. Success stamps `ProcessedOnUtc`; failure records `Error` and leaves the row for the next pass.

The 5-second cadence is why a state change is not always visible to the rest of the system on the very next request.

---

## Saga host

`RideCreatedSaga` and `RideCreatedSagaData` live in `Infrastructure/Sagas/RideCreated/`, registered with an in-memory repository:

```csharp
busConfigurator.AddSagaStateMachine<RideCreatedSaga, RideCreatedSagaData>()
               .InMemoryRepository();
```

The saga lives here because the Ride service is the one with the whole picture — it knows when a ride was requested, started and finished, which are the beats the workflow turns on.

Full walkthrough: **[ride-lifecycle.md](../ride-lifecycle.md)**.

---

## Consumers

`Infrastructure/Consumers/`

| Consumer | Consumes | Sends internally |
|---|---|---|
| `AssignDriverConsumer` | `AssignDriverToRideCommand` | `AssignDriverCommand` |
| `RideStartedConsumer` | `RideStartedEvent` | `StartRideCommand` |
| `CompleteRideConsumer` | `CompleteRideCommand` (contract) | `CompleteRideCommand` (internal) |

Each checks `ProcessedMessages` before acting and logs failures rather than rethrowing.

---

## Persistence

`RideDbContext` — `Rides`, `OutboxMessages`, `ProcessedMessages`. EF Core InMemory database `"Ride"`.

**Persistence model** — `Persistence/Models/Ride.cs` flattens the aggregate: `RiderInfo` becomes `RiderId`/`RiderFirstName`/`RiderLastName`/`RiderEmail`, and each `RideLocation` becomes a latitude/longitude/address triple.

**Configurations** — `RideConfigurations` (key, `Status` stored as `int`), `OutboxMessageConfiguration`, `ProcessedMessageConfiguration` (composite key).

**Repository** — `IRideRepository`: `GetRideByIdAsync`, `CreateRideAsync`, `AssignDriverAsync`, `StartRideAsync`, `CompleteRideAsync`, `CancelRideAsync`. One method per transition rather than a generic `Update`, so each write maps to exactly the columns that transition touches.

**Query** — `IGetRidesQuery` / `GetRidesQuery` projects rows straight to `GetRidesDto`.

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

DI entry point: `Infrastructure/Extensions/IServiceCollectionExtensions.cs` — EF, MediatR, MassTransit + saga, Quartz, Carter, `IDomainEventCollector`, pricing, then Scrutor scans for the `UnitOfWork` / `DateTimeProvider` / `Query` / `Repository` suffixes.

---

## Tests

`ZM.RideService.Api.UnitTests`

| Test file | Cases |
|---|---|
| `CreateRideCommandHandlerTests` | Valid request creates a ride |
| `AssignDriverCommandHandlerTests` | Assigns; ride not found; already assigned |
| `StartRideCommandHandlerTests` | Starts; not found; not assigned |
| `CompleteRideCommandHandlerTests` | Completes; not in progress |
| `CancelRideCommandHandlerTests` | Cancels; not found; already completed |

Builders (`RideBuilder`, `RideLocationBuilder`, `RiderInfoBuilder`) construct aggregates in a named state — `BuildRequested()`, `BuildAssigned()`, `BuildInProgress()`, `BuildCompleted()` — so a guard test states the state it exercises instead of replaying transitions to reach it.
