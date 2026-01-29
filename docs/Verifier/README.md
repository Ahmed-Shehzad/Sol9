# Verifier

Verifier is the **validation framework** used in Sol9. It provides fluent rule builders, sync/async validators, and integration with Intercessor so commands and queries are validated before handlers run.

---

## When to Use

- **Command and query validation**: Validate `IRequest`/`ICommand`/`IQuery` DTOs before they reach handlers (e.g. `CreateOrderCommand`, `GetBookingsQuery`).
- **API input validation**: Validate request bodies or query parameters that are mapped to application commands/queries.
- **Event validation**: Validate integration or domain events when they are consumed (e.g. `OrderCreatedIntegrationEvent`).
- **Reusable rules**: Define `AbstractValidator<T>` once and reuse via Intercessor’s `ValidationBehavior`.

---

## Where to Use

- **Application layer**: Define validators next to commands/queries (e.g. `CreateBookingCommandValidator` in the same folder or assembly as `CreateBookingCommand`).
- **Registration**: In the composition root, register Verifier and point it at the assemblies that contain validators (often done inside **Intercessor** via `AddIntercessor`, which calls `AddVerifier` and scans the same assemblies).
- **Not in Domain**: Domain entities enforce invariants; Verifier is for **application** DTOs and request/event payloads.

---

## Why to Use

- **Fluent API**: `RuleFor(x => x.Name).NotEmpty()`, `Must(...)` keep rules readable and composable.
- **Sync and async**: `RuleFor` + `RuleForAsync` and `MustAsync` for I/O-based rules (e.g. “customer exists”).
- **Structured errors**: `ValidationResult` and `ValidationFailure` with property names and messages; easy to map to API error responses.
- **Integration**: Intercessor’s pipeline runs validators automatically and throws `ValidationException` on failure.

---

## Why Not to Use

- **Domain invariants only**: If you only need to enforce rules inside entities (e.g. “amount > 0” in the entity constructor), you don’t need Verifier for that; use domain logic.
- **Trivial checks**: For a single “not null” check with no custom message, you can validate in the handler; Verifier shines when you have multiple rules or reusable validators.
- **Validation outside application layer**: If you validate only at the API boundary (e.g. DataAnnotations), you can do that without Verifier; Verifier is most useful when the same DTOs are used in handlers and you want one place for rules.

---

## How to Set Up

### 1. Add the package

```bash
dotnet add package Verifier
```

### 2. Register validators

Register Verifier and tell it which assemblies contain `IValidator<T>` implementations:

```csharp
using Verifier;

builder.Services.AddVerifier(options =>
{
    options.RegisterFromAssembly(typeof(MyApplication).Assembly);
    // options.RegisterFromAssembly(Assembly.GetExecutingAssembly());
});
```

In Sol9, **Intercessor** calls `AddVerifier` internally and passes the same assemblies used for handlers, so you usually **don’t** call `AddVerifier` yourself when using Intercessor.

### 3. Create a validator

Implement a validator by inheriting `AbstractValidator<T>` and using `RuleFor` / `RuleForAsync`:

```csharp
using Verifier;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerName)
            .NotEmpty("CustomerName must not be empty.");

        RuleFor(c => c.TotalAmount)
            .Must(amount => amount > 0, "TotalAmount must be greater than zero.");
    }
}
```

---

## Configuration

Verifier itself has no appsettings section. Configuration is done at registration:

| Method | Description |
|--------|-------------|
| `RegisterFromAssembly(Assembly)` | Scan an assembly for classes that implement `IValidator<>` and register them as transients. |

Order of execution: when used with Intercessor, **ValidationBehavior** runs all registered validators for the request type before calling the handler. No extra config is needed for that.

---

## Applying Configuration

- **Single project**: `RegisterFromAssembly(typeof(YourApplication).Assembly)` or `Assembly.GetExecutingAssembly()`.
- **Multiple projects**: Call `RegisterFromAssembly` once per assembly that contains validators (e.g. Application and Domain if events are validated there).

---

## Rule API

### Sync rules (`RuleFor`)

- **NotNull(message?)**: Value must not be null.
- **NotEmpty(message?)**: For string: not null/whitespace; for `Ulid`: not empty.
- **Must(predicate, message)**: Custom condition; predicate receives the property value.

Example:

```csharp
RuleFor(c => c.OrderId)
    .Must(id => id != Ulid.Empty, "OrderId must not be empty.");
```

### Async rules (`RuleForAsync`)

- **MustAsync(predicateAsync, message)**: Async predicate (e.g. database lookup).

Example:

```csharp
RuleForAsync(c => c.CustomerId)
    .MustAsync(async (id, ct) => await _customerExists.CheckAsync(id, ct), "Customer must exist.");
```

---

## Usage with Intercessor

Intercessor registers Verifier and adds `ValidationBehavior` to the pipeline. When a request is sent:

1. All `IValidator<TRequest>` instances are resolved and run (`ValidateAsync`).
2. If any failures are returned, `ValidationException` is thrown with the list of `ValidationFailure`.
3. If valid, the handler runs.

You only need to:

- Implement `AbstractValidator<T>` for your commands/queries/events.
- Register Intercessor with the assembly that contains those validators (and handlers).

---

## Throwing and Handling ValidationException

```csharp
// In Verifier.Exceptions
public sealed class ValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Errors { get; }
}
```

In API exception handling, catch `ValidationException` and map `Errors` to HTTP 400 and a list of property/message pairs.

---

## See Also

- [Intercessor](../Intercessor/README.md) – Uses Verifier for request validation in the pipeline.
