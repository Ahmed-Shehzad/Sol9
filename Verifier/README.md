# Verifier
Lightweight validation library for building and executing rules.

## Purpose
- Define validators and rule builders for request validation.
- Provide standardized validation results and exceptions.

## Key types
- `AbstractValidator<T>`
- `RuleBuilder<T, TProperty>`
- `VerifierBuilder`
- `ValidationResult`, `ValidationFailure`
- `ValidationException`

## Registration
```csharp
services.AddVerifier(builder =>
{
    builder.RegisterFromAssembly(typeof(SomeValidator).Assembly);
});
```
