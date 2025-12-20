using Verifier.Abstractions;

namespace Verifier;

public sealed class RuleBuilder<T, TProperty> : IValidationRule<T>
{
    private readonly Func<T, TProperty> _getter;
    private readonly string _propertyName;

    // Each rule returns a failure or null (null == pass)
    private readonly List<Func<T, ValidationFailure?>> _rules = [];

    public RuleBuilder(string propertyName, Func<T, TProperty> getter)
    {
        _propertyName = propertyName;
        _getter = getter;
    }

    public RuleBuilder<T, TProperty> NotNull(string? message = null)
    {
        _rules.Add(obj =>
        {
            var value = _getter(obj);
            if (value is null)
                return new ValidationFailure(
                    _propertyName, message ?? $"{_propertyName} must not be null.");

            return null;
        });
        return this;
    }

    public RuleBuilder<T, TProperty> NotEmpty(string? message = null)
    {
        _rules.Add(obj =>
        {
            var value = _getter(obj);

            var isEmpty =
                value is null ||
                (value is string s && string.IsNullOrWhiteSpace(s));

            if (isEmpty)
                return new ValidationFailure(
                    _propertyName, message ?? $"{_propertyName} must not be empty.");

            return null;
        });
        return this;
    }

    public RuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate, string message)
    {
        _rules.Add(obj =>
        {
            var value = _getter(obj);
            if (!predicate(value))
            {
                return new ValidationFailure(_propertyName, message);
            }

            return null;
        });
        return this;
    }

    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        foreach (var rule in _rules)
        {
            var failure = rule(instance);
            if (failure is not null) yield return failure;
        }
    }

}
