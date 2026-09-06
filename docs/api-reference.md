# API reference

Twelve HTTP endpoints across three services. Matching and Notification expose none — they are reached only through RabbitMQ ([messaging.md](messaging.md)).

- [Conventions](#conventions)
- [Ride Service](#ride-service--5000)
- [Driver Service](#driver-service--5100)
- [Payment Service](#payment-service--5300)
- [End-to-end walkthrough](#end-to-end-walkthrough)

---

## Conventions

**Carter modules, not controllers.** Endpoints are declared in `ICarterModule.AddRoutes` and mapped by `app.MapCarter()`:

```csharp
app.MapPost("api/CreateRide", async ([FromBody] CreateRideCommand request, ISender sender) =>
{
    var ride = await sender.Send(request);
    return Results.Ok(ride);
});
```

The endpoint binds the body straight to the MediatR command and delegates. No mapping layer, no controller — the use case *is* the request shape.

**The response envelope.** Handlers return `Result` or `Result<T>`, serialized into the 200 body. Callers branch on `isSuccessful`, not on the status code:

```jsonc
// success with data
{ "data": [ /* ... */ ], "isSuccessful": true, "message": null, "error": null }

// success without data
{ "isSuccessful": true, "message": null, "error": null }

// business rule violation
{
  "isSuccessful": false,
  "message": null,
  "error": { "errorCode": "RIDE0003", "errorMessage": "Ride could not be started because it is not in the driver assigned status." }
}
```

`errorCode` is the stable identifier to branch on; `errorMessage` is for humans. The full catalogue is in [architecture.md](architecture.md#result-and-error).

**Routes** are `api/PascalCaseVerb` — action-named rather than resource-named, matching the use-case-per-folder structure. There is no route versioning and no authentication; every endpoint is anonymous.

**Swagger UI** is available on every service at `/swagger`.

---

## Ride Service — :5000

[ZM.RideService.Api/Presentation/RideModule.cs](../ZM.RideService.Api/Presentation/RideModule.cs)

### `GET api/GetRides`

All rides. Returns `Result<IEnumerable<GetRidesDto>>`, projected straight from storage by `IGetRidesQuery` without rebuilding aggregates.

```jsonc
{
  "data": [
    {
      "id": "8f3c...", "riderId": "1a2b...", "driverId": "9e8d...",
      "pickupLocation":      { "latitude": 44.7866, "longitude": 20.4489, "address": "Trg Republike, Belgrade" },
      "destinationLocation": { "latitude": 44.8125, "longitude": 20.4612, "address": "Kalemegdan, Belgrade" },
      "status": 2,
      "estimatedFare": 6.42, "actualFare": null,
      "createdAtUtc": "2026-09-06T10:15:00Z", "assignedAtUtc": "2026-09-06T10:15:07Z",
      "startedAtUtc": null, "completedAtUtc": null, "cancelledAtUtc": null
    }
  ],
  "isSuccessful": true
}
```

`status` is the numeric `RideStatus`: `1` Requested, `2` DriverAssigned, `3` DriverArrived, `4` InProgress, `5` Completed, `6` Cancelled.

### `POST api/CreateRide`

Creates a ride, prices an estimate, and starts the saga.

`CreateRideCommand(RiderInfo Rider, RideLocation PickupLocation, RideLocation DestinationLocation)`

```json
{
  "rider": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Ana",
    "lastName": "Jovanovic",
    "email": "ana.jovanovic@example.com"
  },
  "pickupLocation":      { "latitude": 44.7866, "longitude": 20.4489, "address": "Trg Republike, Belgrade" },
  "destinationLocation": { "latitude": 44.8125, "longitude": 20.4612, "address": "Kalemegdan, Belgrade" }
}
```

Returns `Result`. The generated ride id is not in the response — read it back from `GET api/GetRides`.

The rider's `email` is what every later notification is addressed to: it travels into `RideCreatedEvent` and is held in saga state for the whole workflow.

### `PUT api/AssignDriver`

`AssignDriverCommand(Guid RideId, Guid DriverId)`

```json
{ "rideId": "8f3c...", "driverId": "9e8d..." }
```

Requires `Requested`, else `RIDE0002`. The saga normally performs this step; the endpoint exists for manual assignment.

### `PUT api/StartRide`

`StartRideCommand(Guid RideId)`

```json
{ "rideId": "8f3c..." }
```

Requires `DriverAssigned`, else `RIDE0003`. Stamps `StartedAtUtc`, which the final fare's time component is measured from.

### `PUT api/CompleteRide`

`CompleteRideCommand(Guid RideId)`

```json
{ "rideId": "8f3c..." }
```

Requires `InProgress`, else `RIDE0004`. Computes the actual fare via `IFareCalculator`, then fans out notification, driver-release and payment messages.

---

## Driver Service — :5100

[ZM.DriverService.Api/Presentation/DriverModule.cs](../ZM.DriverService.Api/Presentation/DriverModule.cs)

### `GET api/GetDrivers`

Returns `Result<IEnumerable<GetDriversDto>>`.

```jsonc
{
  "data": [
    {
      "id": "9e8d...", "firstName": "Marko", "lastName": "Petrovic",
      "email": "marko.petrovic@example.com", "phoneNumber": "+381601234567",
      "status": 2, "currentRideId": null,
      "currentLatitude": 44.7900, "currentLongitude": 20.4500,
      "createdAtUtc": "2026-09-06T10:00:00Z",
      "lastLocationUpdateAtUtc": "2026-09-06T10:14:30Z",
      "lastStatusChangeAtUtc": "2026-09-06T10:14:30Z"
    }
  ],
  "isSuccessful": true
}
```

`status` is the numeric `DriverStatus`: `1` Offline, `2` Available, `3` Assigned, `4` InRide.

### `POST api/RegisterDriver`

`RegisterDriverCommand(string FirstName, string LastName, string Email, string PhoneNumber)`

```json
{
  "firstName": "Marko",
  "lastName": "Petrovic",
  "email": "marko.petrovic@example.com",
  "phoneNumber": "+381601234567"
}
```

The driver is created `Offline` and is not yet matchable — registering and going on shift are separate acts.

### `PUT api/SetDriverAvailable`

`SetDriverAvailableCommand(Guid DriverId, double Latitude, double Longitude)`

```json
{ "driverId": "9e8d...", "latitude": 44.7900, "longitude": 20.4500 }
```

**This is what makes a driver matchable.** It raises both `DriverAvailableDomainEvent` and `DriverLocationUpdatedDomainEvent`, which reach the Matching service and populate the Redis index. No ride can be matched to a driver who has not called this.

Rejected with `DRIVER0002` if the driver is `InRide`. Calling it on an already-`Available` driver succeeds as a no-op without re-raising events.

### `PUT api/SetDriverUnavailable`

`SetDriverUnavailableCommand(Guid DriverId)`

```json
{ "driverId": "9e8d..." }
```

Returns the driver to `Offline` and removes them from the matching index. Rejected with `DRIVER0002` mid-ride — a driver cannot go offline while carrying a passenger.

### `PUT api/UpdateDriverLocation`

`UpdateDriverLocationCommand(Guid DriverId, double Latitude, double Longitude)`

```json
{ "driverId": "9e8d...", "latitude": 44.7912, "longitude": 20.4523 }
```

The GPS ping. Unguarded by design — position is a fact, not a state transition — so it always succeeds and always publishes `DriverLocationUpdatedEvent`. Whether the ping moves the driver in the matching index is decided downstream in Redis, by whether they are currently available.

---

## Payment Service — :5300

[ZM.PaymentService.Api/Presentation/PaymentModule.cs](../ZM.PaymentService.Api/Presentation/PaymentModule.cs)

Read-only. Payments are created by `ProcessPaymentCommand` from the saga, never over HTTP.

### `GET api/GetPaymentById/{id}`

```
GET /api/GetPaymentById/5c1f8e2a-91b4-4d3e-8f77-2a6c4b9e0d31
```

Returns `Result<GetPaymentByIdDto>`, or `PAY0001` if not found.

```jsonc
{
  "data": {
    "id": "5c1f...", "rideId": "8f3c...", "amount": 8.90,
    "status": 1,
    "createdAtUtc": "2026-09-06T10:32:00Z",
    "processedAtUtc": "2026-09-06T10:32:00Z"
  },
  "isSuccessful": true
}
```

`status` is the numeric `PaymentStatus`: `0` Pending, `1` Completed, `2` Failed.

### `GET api/GetPaymentByRideId/{rideId}`

```
GET /api/GetPaymentByRideId/8f3c2d19-4e5a-4b8c-9d1f-7a3e6c2b5f04
```

The most recent payment for a ride, ordered by `CreatedAtUtc` descending. Same DTO shape.

---

## End-to-end walkthrough

Order matters — a driver must be available before a ride can match. Adjust ports if you are running outside Compose.

```bash
# 1. Register a driver (returns Result only; read the id back in step 2)
curl -X POST http://localhost:5100/api/RegisterDriver \
  -H "Content-Type: application/json" \
  -d '{
        "firstName": "Marko",
        "lastName": "Petrovic",
        "email": "marko.petrovic@example.com",
        "phoneNumber": "+381601234567"
      }'

# 2. Read the generated driver id
curl http://localhost:5100/api/GetDrivers

# 3. Put the driver on shift — this populates the Redis matching index
curl -X PUT http://localhost:5100/api/SetDriverAvailable \
  -H "Content-Type: application/json" \
  -d '{ "driverId": "<DRIVER_ID>", "latitude": 44.7900, "longitude": 20.4500 }'

# 4. Request a ride, within 10 km of the driver
curl -X POST http://localhost:5000/api/CreateRide \
  -H "Content-Type: application/json" \
  -d '{
        "rider": {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "firstName": "Ana",
          "lastName": "Jovanovic",
          "email": "ana.jovanovic@example.com"
        },
        "pickupLocation":      { "latitude": 44.7866, "longitude": 20.4489, "address": "Trg Republike, Belgrade" },
        "destinationLocation": { "latitude": 44.8125, "longitude": 20.4612, "address": "Kalemegdan, Belgrade" }
      }'

# 5. Read the ride id and watch matching and assignment land
curl http://localhost:5000/api/GetRides

# 6. Start the ride
curl -X PUT http://localhost:5000/api/StartRide \
  -H "Content-Type: application/json" \
  -d '{ "rideId": "<RIDE_ID>" }'

# 7. Complete it — this triggers notification, driver release and payment
curl -X PUT http://localhost:5000/api/CompleteRide \
  -H "Content-Type: application/json" \
  -d '{ "rideId": "<RIDE_ID>" }'

# 8. Read the payment
curl http://localhost:5300/api/GetPaymentByRideId/<RIDE_ID>
```

Between steps, two things are worth watching:

- **RabbitMQ** at http://localhost:15672 (`admin`/`admin`) — the queues correspond one-to-one with the consumers in [messaging.md](messaging.md).
- **Service logs** — the Notification service writes the rendered e-mail bodies to its log, so `docker compose logs -f zm.notificationservice.api` shows the messages a rider would receive.

Steps 4 through 7 involve the outbox, which drains on a 5-second Quartz schedule, so allow a moment for each transition to propagate rather than expecting the next `GET` to reflect it immediately.
