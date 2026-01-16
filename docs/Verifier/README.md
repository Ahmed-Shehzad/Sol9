# Verifier

Verifier is a lightweight validation library for building and executing validation rules in .NET applications.

## Overview

Verifier provides:
- Fluent rule builders for declarative validation
- Integration with Intercessor for automatic validation
- Standardized validation results and exceptions
- Support for synchronous and asynchronous validation

## Core Concepts

### Validators

Validators define rules for request types:

```csharp
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.TotalAmount)
            .Must(amount => amount > 0, "TotalAmount must be greater than zero");
    }
}
```

### Rules

Rules are built using fluent syntax:

- `NotNull()`: Value must not be null
- `NotEmpty()`: Value must not be null or empty
- `Must()`: Custom validation predicate
- `AsyncMust()`: Async validation predicate

### Validation Results

Validation produces `ValidationResult` with `ValidationFailure` objects:

```csharp
var result = await validator.ValidateAsync(command);
if (!result.IsValid)
{
    foreach (var failure in result.Failures)
    {
        Console.WriteLine($"{failure.PropertyName}: {failure.ErrorMessage}");
    }
}
```

## Getting Started

### 1. Install Verifier

```bash
dotnet add package Verifier
```

### 2. Register Services

In your `Program.cs`:

```csharp
using Verifier;

builder.Services.AddVerifier(options =>
{
    // Register validators from assembly
    options.RegisterFromAssembly(typeof(Program).Assembly);
});
```

### 3. Create Validators

```csharp
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.TotalAmount)
            .Must(amount => amount > 0, "TotalAmount must be greater than zero");
    }
}
```

### 4. Use with Intercessor

Verifier automatically integrates with Intercessor:

```csharp
// Register both
builder.Services.AddVerifier(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
    // ValidationBehavior is automatically added
});
```

## When to Use

### Use Validators When:
- You need to validate command/query input
- You want consistent validation across your application
- You need to return detailed validation errors
- Example: Validating `CreateOrderCommand` before processing

### Use Rule Builders When:
- You need declarative validation rules
- You want reusable validation logic
- You need complex validation scenarios
- Example: Validating email format, date ranges, etc.

## Where to Use

### Application Layer

**Validators**: Place next to commands/queries they validate.

```
Orders.Application/
├── Commands/
│   └── CreateOrder/
│       ├── CreateOrderCommand.cs
│       ├── CreateOrderCommandHandler.cs
│       └── CreateOrderCommandValidator.cs  ← Validator here
└── Queries/
    └── GetOrder/
        ├── GetOrderQuery.cs
        ├── GetOrderQueryHandler.cs
        └── GetOrderQueryValidator.cs  ← Validator here
```

## Use Cases

### 1. Command Validation

**Scenario**: Validate order creation command.

```csharp
// Command
public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) 
    : ICommand<OrderDto>;

// Validator
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.TotalAmount)
            .Must(amount => amount > 0, "TotalAmount must be greater than zero")
            .Must(amount => amount <= 1000000, "TotalAmount exceeds maximum");
    }
}

// Handler - validation happens automatically via ValidationBehavior
public sealed class CreateOrderCommandHandler 
    : ICommandHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        // Command is already validated
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        // ...
    }
}
```

### 2. Complex Validation

**Scenario**: Validate with custom rules and async checks.

```csharp
public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    private readonly IBookingsRepository _repository;

    public CreateBookingCommandValidator(IBookingsRepository repository)
    {
        _repository = repository;
        
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .MustAsync(async (orderId, cancellationToken) =>
            {
                // Async validation: check if order exists
                var order = await _repository.GetOrderAsync(orderId, cancellationToken);
                return order != null;
            }, "Order not found");
            
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(100)
            .Must(name => !name.Contains("test"), "Invalid customer name");
    }
}
```

### 3. Property-Level Validation

**Scenario**: Validate nested properties.

```csharp
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.ShippingAddress).NotNull();
        
        // Validate nested properties
        RuleFor(x => x.ShippingAddress.Street)
            .NotEmpty()
            .When(x => x.ShippingAddress != null);
            
        RuleFor(x => x.ShippingAddress.PostalCode)
            .Matches(@"^\d{5}(-\d{4})?$")
            .When(x => x.ShippingAddress != null);
    }
}
```

## Rule Builder Methods

### Basic Rules

```csharp
RuleFor(x => x.PropertyName)
    .NotNull()                    // Must not be null
    .NotEmpty()                   // Must not be null or empty
    .Must(predicate, message)     // Custom predicate
    .AsyncMust(asyncPredicate, message)  // Async predicate
```

### String Rules

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()               // Valid email format
    .MaximumLength(255)
    .MinimumLength(5);
```

### Numeric Rules

```csharp
RuleFor(x => x.Age)
    .GreaterThan(0)
    .LessThan(150);
    
RuleFor(x => x.Price)
    .GreaterThanOrEqualTo(0)
    .LessThanOrEqualTo(10000);
```

### Collection Rules

```csharp
RuleFor(x => x.Items)
    .NotNull()
    .NotEmpty()
    .Must(items => items.Count <= 10, "Maximum 10 items allowed");
```

## Integration with Intercessor

Verifier automatically integrates with Intercessor's `ValidationBehavior`:

```csharp
// Register both
builder.Services.AddVerifier(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
    // ValidationBehavior is automatically registered
});

// Validation happens automatically
public async Task<IActionResult> Create(CreateOrderCommand command)
{
    // If validation fails, ValidationException is thrown
    // Intercessor's ValidationBehavior handles it
    var order = await _sender.SendAsync(command);
    return Ok(order);
}
```

## Error Handling

### ValidationException

When validation fails, `ValidationException` is thrown:

```csharp
try
{
    var result = await _sender.SendAsync(command);
}
catch (ValidationException ex)
{
    // Access validation failures
    foreach (var failure in ex.ValidationResult.Failures)
    {
        ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
    }
    return BadRequest(ModelState);
}
```

### Custom Error Responses

Handle validation errors in exception handler:

```csharp
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                errors = validationException.ValidationResult.Failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray())
            });
        }
    });
});
```

## Best Practices

1. **One Validator Per Request**: Create a validator for each command/query
2. **Place Validators Next to Requests**: Keep validators close to what they validate
3. **Use Descriptive Messages**: Provide clear error messages
4. **Validate Early**: Validate in the pipeline, not in handlers
5. **Use Async Validation Sparingly**: Only for expensive operations
6. **Keep Rules Simple**: Complex logic should be in handlers
7. **Reuse Common Rules**: Extract common validation into extension methods

## Advanced Usage

### Custom Validators

Create reusable validators:

```csharp
public static class ValidationExtensions
{
    public static IRuleBuilder<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
    }
}

// Use
RuleFor(x => x.Email).ValidEmail();
```

### Conditional Validation

```csharp
RuleFor(x => x.ShippingAddress)
    .NotNull()
    .When(x => x.RequiresShipping);

RuleFor(x => x.BillingAddress)
    .NotNull()
    .When(x => !x.RequiresShipping);
```

## See Also

- [Intercessor Documentation](../Intercessor/README.md) - Mediator framework with validation integration
