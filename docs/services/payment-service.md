# Payment Service

`ZM.PaymentService.Api` — port **5300** (local: 5107 / 7271)

Records the settlement of a completed ride. The payment step is the saga's final beat: its completion event is what releases the receipt and ends the workflow.

- [Domain model](#domain-model)
- [Endpoints](#endpoints)
- [Use cases](#use-cases)
- [Consumers](#consumers)
- [Persistence](#persistence)
- [Configuration](#configuration)
- [Tests](#tests)

---

## Domain model

### `Payment` aggregate

[Domain/Entities/Payment.cs](../../ZM.PaymentService.Api/Domain/Entities/Payment.cs)

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | |
| `RideId` | `Guid` | The ride being settled |
| `Amount` | `decimal` | The ride's actual fare |
| `Status` | `PaymentStatus` | |
| `CreatedAtUtc` | `DateTime` | When the attempt began |
| `ProcessedAtUtc` | `DateTime?` | When it resolved, either way |

The smallest aggregate in the system, and the only one whose entire lifecycle is a single decision: did the money move or not.

**Behaviour and guards:**

| Method | Requires | Raises | Failure |
|---|---|---|---|
| `Create` | — | — | — |
| `Complete` | not already `Completed` | `PaymentCompletedDomainEvent` | `PAY0002` |
| `Fail` | not already `Failed` | — | `PAY0002` |
| `Rehydrate` | — | — | — |

Both terminal transitions guard against repeating themselves. That matters more here than anywhere else in the codebase: settlement messages can be redelivered, and completing a payment twice would raise `PaymentCompletedDomainEvent` twice, sending the rider two receipts. The guard makes the second attempt a no-op that reports why.

`Complete` takes `recipientEmail` purely to carry it into the event — the aggregate does not store it. The Payment service never needs to know who the rider is; it only needs to pass the address along so the Notification service can address the receipt.

### `PaymentStatus`

```csharp
Pending = 0, Completed = 1, Failed = 2
```

`Pending` at `0` makes it the default value, so a payment that has not resolved reads as pending rather than as anything more definite.

### Domain event

```csharp
public record PaymentCompletedDomainEvent(Guid PaymentId, Guid RideId, decimal Amount, string RecipientEmail);
```

`PaymentCompletedDomainEventHandler` translates it to the published `PaymentCompletedEvent` — the last event the saga waits for.

---

## Endpoints

[Presentation/PaymentModule.cs](../../ZM.PaymentService.Api/Presentation/PaymentModule.cs)

| Verb | Route | Query |
|---|---|---|
| GET | `api/GetPaymentById/{id}` | `GetPaymentByIdQuery(Guid PaymentId)` |
| GET | `api/GetPaymentByRideId/{rideId}` | `GetPaymentByRideIdQuery(Guid RideId)` |

**Read-only by design.** Payments are created by `ProcessPaymentCommand` from the saga, never over HTTP. There is no endpoint to charge a rider, because settlement is a consequence of completing a ride rather than something a client initiates. Both routes take their parameter from the path rather than a body, being genuine reads.

`GetPaymentByRideId` returns the most recent payment for a ride, ordered by `CreatedAtUtc` descending — the shape allows for a ride accumulating more than one attempt.

Response shapes: [api-reference.md](../api-reference.md#payment-service--5300).

---

## Use cases

| Use case | Handler | Trigger |
|---|---|---|
| ProcessPayment | `ProcessPaymentCommandHandler` | Message from the saga |
| GetPaymentById | `GetPaymentByIdQueryHandler` | HTTP |
| GetPaymentByRideId | `GetPaymentByRideIdQueryHandler` | HTTP |

### `ProcessPaymentCommandHandler`

[Application/UseCases/ProcessPayment/ProcessPaymentCommandHandler.cs](../../ZM.PaymentService.Api/Application/UseCases/ProcessPayment/ProcessPaymentCommandHandler.cs)

```csharp
var payment = Payment.Create(Guid.NewGuid(), request.RideId, request.Amount, _dateTimeProvider.UtcNow);

var completeResult = payment.Complete(request.RecipientEmail, _dateTimeProvider.UtcNow);
if (completeResult.IsSuccessful is false)
{
    return completeResult;
}

await _paymentRepository.CreatePaymentAsync(payment, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

Create, then complete, then persist — the payment exists in `Pending` only in memory, for the moment between the two calls.

**Settlement is simulated.** There is no payment gateway, no card tokenisation, no 3-D Secure step, and no external call of any kind: the amount is taken as given and the payment completes. The aggregate is nonetheless modelled for the real thing — `Pending` and `Fail` exist, `Complete` guards against double-settlement, and `ProcessedAtUtc` is distinct from `CreatedAtUtc` precisely because a real gateway call takes time and can fail.

Introducing a provider means giving the handler an `IPaymentGateway`, awaiting its result, and calling `payment.Complete(...)` or `payment.Fail(...)` on the answer. The aggregate, the events and the saga's contract with this service all stay as they are — which is the point of having modelled the failure path before there was anything that could fail.

### Read handlers

Both load through `IPaymentRepository` and project to a DTO, returning `PAY0001` when nothing is found:

```csharp
public record GetPaymentByIdDto(Guid Id, Guid RideId, decimal Amount,
                                PaymentStatus Status, DateTime CreatedAtUtc, DateTime? ProcessedAtUtc);
```

`GetPaymentByRideIdDto` has the same shape. Keeping them separate rather than sharing one type lets the two reads diverge later without one caller's needs constraining the other's.

---

## Consumers

| Consumer | Consumes |
|---|---|
| `ProcessPaymentConsumer` | `ProcessPaymentCommand` |

The consumer checks `ProcessedMessages` for `(MessageId, ConsumerName)` before acting, then delegates to `ProcessPaymentCommandHandler`.

---

## Persistence

`PaymentDbContext` — `Payments`, `ProcessedMessages`. EF Core InMemory database `"Payment"`.

**Persistence model** — `Persistence/Models/Payment.cs`. Note the storage column is named `CompletedAtUtc` while the domain property is `ProcessedAtUtc`; the repository maps between them. The domain name is the more accurate of the two, since the timestamp is stamped by `Fail` as well as `Complete`.

**Repository** — `IPaymentRepository`: `CreatePaymentAsync`, `GetPaymentByIdAsync`, `GetPaymentByRideIdAsync`, `UpdatePaymentAsync`.

**Configurations** — `ProcessedMessageConfiguration` (composite key) is the only explicit one; `Payment` is mapped by EF Core convention, its `Id` picked up as the key without configuration. Both are discovered through `ApplyConfigurationsFromAssembly` in `OnModelCreating`.

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

DI: EF, MediatR, MassTransit with one consumer, Carter, and Scrutor scans for the `UnitOfWork` / `DateTimeProvider` / `Query` / `Repository` suffixes.

---

## Tests

`ZM.PaymentService.Api.UnitTests`

| Test file | Cases |
|---|---|
| `UseCases/ProcessPaymentCommandHandlerTests` | Valid request processes a payment |

`PaymentBuilder` constructs payments in a given state. `UnitTest1.cs` is leftover project scaffolding.
