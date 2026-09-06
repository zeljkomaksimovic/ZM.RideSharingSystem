# Driver Service

`ZM.DriverService.Api` — port **5100** (local: 5231 / 7109)

Owns driver identity, shift status and position. It is the write side of driver state; the Matching service maintains the searchable index from the events published here.

- [Domain model](#domain-model)
- [Endpoints](#endpoints)
- [Use cases](#use-cases)
- [Consumers](#consumers)
- [Event dispatch](#event-dispatch)
- [Persistence](#persistence)
- [Configuration](#configuration)
- [Tests](#tests)

---

## Domain model

### `Driver` aggregate

[Domain/Entities/Driver.cs](../../ZM.DriverService.Api/Domain/Entities/Driver.cs)

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | |
| `FirstName`, `LastName` | `string` | |
| `Email` | `string` | |
| `PhoneNumber` | `string` | |
| `CurrentRideId` | `Guid?` | Set on assignment, cleared on completion |
| `CurrentLocation` | `DriverLocation?` | Null until the driver first goes available |
| `Status` | `DriverStatus` | |
| `CreatedAtUtc` | `DateTime` | |
| `LastLocationUpdateAtUtc` | `DateTime?` | |
| `LastStatusChangeAtUtc` | `DateTime?` | |

Two timestamps rather than one, because position and status change at very different rates — a driver pings location constantly while sitting in one status for an hour.

**Behaviour and guards:**

| Method | Requires | Raises | Failure |
|---|---|---|---|
| `Create` | — | — | — |
| `SetAvailable` | not `InRide` | `DriverAvailableDomainEvent` + `DriverLocationUpdatedDomainEvent` | `DRIVER0002` |
| `SetUnavailable` | not `InRide` | `DriverUnavailableDomainEvent` | `DRIVER0002` |
| `AssignRide` | `Available` | `RideAssignedDomainEvent` | `DRIVER0003` |
| `StartRide` | `Assigned` | — | `DRIVER0004` |
| `CompleteRide` | `InRide` | — | `DRIVER0005` |
| `UpdateLocation` | — | `DriverLocationUpdatedDomainEvent` | — |

Three details worth drawing out:

**`SetAvailable` raises two events.** Going on shift is both a status change and a position report, and the Matching service needs both to index the driver — set membership from one, geo position from the other.

**`SetAvailable` is idempotent.** A driver already `Available` returns success without raising anything:

```csharp
if (Status == DriverStatus.Available)
{
    return Result.Success();
}
```

Repeated availability pings from a mobile client are therefore free rather than flooding the bus.

**`UpdateLocation` returns `void`, not `Result`.** It is the only unguarded method on the aggregate: position is a fact about the world rather than a state transition, so there is no invariant to violate. Whether the ping reaches the matching index is decided downstream in Redis, by whether the driver is currently available.

**`CompleteRide` returns the driver to `Available`, not `Offline`** — finishing a ride keeps them on shift and immediately matchable again.

### Value object and enum

```csharp
public record DriverLocation(double Latitude, double Longitude);
```

```csharp
Offline = 1, Available = 2, Assigned = 3, InRide = 4
```

Note `DriverLocation` has no `Address`, unlike the Ride service's `RideLocation` — a driver's position is a GPS reading, while a ride's pickup point is something a human chose and named.

### Domain events

`Domain/Events/`:

| Event | Payload |
|---|---|
| `DriverAvailableDomainEvent` | `(Guid DriverId)` |
| `DriverUnavailableDomainEvent` | `(Guid DriverId)` |
| `DriverLocationUpdatedDomainEvent` | `(Guid DriverId, double Latitude, double Longitude)` |
| `RideAssignedDomainEvent` | `(Guid RideId, Guid DriverId)` |

---

## Endpoints

[Presentation/DriverModule.cs](../../ZM.DriverService.Api/Presentation/DriverModule.cs)

| Verb | Route | Command |
|---|---|---|
| GET | `api/GetDrivers` | `GetDriversRequest` |
| POST | `api/RegisterDriver` | `RegisterDriverCommand(string, string, string, string)` |
| PUT | `api/SetDriverAvailable` | `SetDriverAvailableCommand(Guid, double, double)` |
| PUT | `api/SetDriverUnavailable` | `SetDriverUnavailableCommand(Guid)` |
| PUT | `api/UpdateDriverLocation` | `UpdateDriverLocationCommand(Guid, double, double)` |

`SetDriverAvailable` is the entry point to the whole matching pipeline — no ride can be matched to a driver who has not called it. Request bodies: [api-reference.md](../api-reference.md#driver-service--5100).

---

## Use cases

| Use case | Handler | Trigger |
|---|---|---|
| RegisterDriver | `RegisterDriverCommandHandler` | HTTP |
| SetDriverAvailable | `SetDriverAvailableCommandHandler` | HTTP |
| SetDriverUnavailable | `SetDriverUnavailableCommandHandler` | HTTP |
| UpdateDriverLocation | `UpdateDriverLocationCommandHandler` | HTTP |
| AssignRide | `AssignRideCommandHandler` | Message |
| StartRide | `StartRideCommandHandler` | Message |
| CompleteRide | `CompleteRideCommandHandler` | Message |
| GetDrivers | `GetDriversRequestHandler` | HTTP |

The split is meaningful: a driver controls their own registration, shift and position over HTTP, while ride assignment and progression arrive as messages from the saga. A driver cannot put themselves into a ride.

**Domain-event handlers** publish the integration events:

| Handler | Publishes |
|---|---|
| `DriverAvailableDomainEventHandler` | `DriverAvailableEvent` |
| `DriverUnavailableDomainEventHandler` | `DriverUnavailableEvent` |
| `DriverLocationUpdatedDomainEventHandler` | `DriverLocationUpdatedEvent` |
| `RideAssignedDomainEventHandler` | `DriverAssignedEvent` |

`RideAssignedDomainEventHandler` is the saga's confirmation that the driver accepted the ride — the beat that moves it from `AwaitingDriverAssignment` to `AwaitingRideToStart`.

---

## Consumers

`Infrastructure/Consumers/`

| Consumer | Consumes | Effect |
|---|---|---|
| `AssignRideToDriverConsumer` | `AssignRideToDriverCommand` | `AssignRide(...)` → `Assigned` |
| `DriverStartRideConsumer` | `DriverStartRideEvent` | `StartRide(...)` → `InRide` |
| `DriverCompleteRideConsumer` | `DriverCompleteRideEvent` | `CompleteRide(...)` → `Available` |

---

## Event dispatch

This service publishes domain events **directly** rather than through an outbox:

```csharp
public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
{
    await _dbContext.SaveChangesAsync(cancellationToken);

    foreach (var domainEvent in _domainEventCollector.GetDomainEvents())
    {
        await _publisher.Publish(domainEvent, cancellationToken);
    }

    _domainEventCollector.ClearDomainEvents();
}
```

Save first, then publish. Compared with the Ride service's outbox ([architecture.md](../architecture.md#transactional-outbox)) this is simpler and lower-latency — events go out immediately rather than waiting up to 5 seconds for a Quartz pass — at the cost of the store-and-forward guarantee: a crash between the save and the publish loses the event.

Having both implementations side by side in one codebase makes the trade-off concrete. Driver events are frequent, individually low-value position and status pings where a lost message is corrected by the next one seconds later; ride events are rare and each one is a step in a workflow that cannot proceed without it. The stronger guarantee is spent where losing a message actually costs something.

---

## Persistence

`DriverDbContext` — `Drivers`, `ProcessedMessages`. EF Core InMemory database `"Driver"`.

**Persistence model** — `Persistence/Models/Driver.cs` flattens `DriverLocation` into nullable `CurrentLatitude` / `CurrentLongitude`.

**Configurations** — `DriverConfigurations` (key, `Status` as `int`), `ProcessedMessageConfiguration` (composite key).

**Repository** — `IDriverRepository`: `CreateDriverAsync`, `GetDriverByIdAsync`, `UpdateDriverAsync`, `DriverExistsAsync`, `GetAvailableDriversAsync`.

**Query** — `IGetDriversQuery` / `GetDriversQuery` → `GetDriversDto`.

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

No Redis connection — this service publishes driver state and never queries the index built from it.

---

## Tests

`ZM.DriverService.Api.UnitTests`

| Test file | Cases |
|---|---|
| `RegisterDriverCommandHandlerTests` | Registers a driver |
| `SetDriverAvailableCommandHandlerTests` | Sets available; not found; in ride |
| `SetDriverUnavailableCommandHandlerTests` | Sets unavailable; not found; in ride |
| `AssignRideCommandHandlerTests` | Assigns; not found; not available |
| `StartRideCommandHandlerTests` | Starts; not found; not assigned |
| `CompleteRideCommandHandlerTests` | Completes; not found; not in ride |
| `UpdateDriverLocationCommandHandlerTests` | Updates; not found |

Builders: `DriverBuilder`, `DriverLocationBuilder`.
