using System.Reflection;

namespace BuildingBlocks.Utilities.Types;

public abstract class Enumeration(long key, string value) : IComparable
{
    /// <summary>
    /// Gets the unique identifier of the enumeration value.
    /// </summary>
    public long Key
    {
        get => key;
    }

    /// <summary>
    /// Gets the string representation of the enumeration value.
    /// </summary>
    public string Value
    {
        get => value;
    }

    /// <summary>
    /// Returns a string representation of the Enumeration's value.
    /// </summary>
    /// <returns>The string value of the Enumeration.</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Retrieves all instances of the specified Enumeration type.
    /// </summary>
    /// <typeparam name="T">The Enumeration type to retrieve instances for.</typeparam>
    /// <returns>An IEnumerable of all instances of the specified Enumeration type.</returns>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var info in fields)
        {
            var instance = new T();

            if (info.GetValue(instance) is T locatedValue)
            {
                yield return locatedValue;
            }
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        // Check if the types of the current object and the specified object are the same
        var typeMatches = GetType() == obj.GetType();

        // Check if the unique identifier of the current object is equal to the unique identifier of the specified object
        var valueMatches = key.Equals(otherValue.Key);

        // Return true if both type and value match, otherwise return false
        return typeMatches && valueMatches;
    }

    /// <summary>
    /// Returns the hash code for the current Enumeration instance.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer hash code calculated based on the unique identifier of the Enumeration.
    /// </returns>
    public override int GetHashCode()
    {
        return key.GetHashCode();
    }

    /// <summary>
    /// Calculates the absolute difference between two Enumeration instances based on their unique identifiers.
    /// </summary>
    /// <param name="firstValue">The first Enumeration instance.</param>
    /// <param name="secondValue">The second Enumeration instance.</param>
    /// <returns>The absolute difference between the unique identifiers of the two Enumeration instances.</returns>
    public static long AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Key - secondValue.Key);
        return absoluteDifference;
    }

    /// <summary>
    /// Retrieves an instance of the specified Enumeration type based on the provided unique identifier.
    /// </summary>
    /// <typeparam name="T">The Enumeration type to retrieve an instance for.</typeparam>
    /// <param name="key">The unique identifier of the Enumeration instance to retrieve.</param>
    /// <returns>The Enumeration instance with the specified unique identifier.</returns>
    /// <exception cref="ApplicationException">Thrown when no Enumeration instance with the specified unique identifier is found.</exception>
    public static T FromId<T>(long key) where T : Enumeration, new()
    {
        var matchingItem = Parse<T, long>(key, "key", item => item.Key == key);
        return matchingItem;
    }
    
    /// <summary>
    /// Retrieves an instance of the specified Enumeration type based on the provided string value.
    /// </summary>
    /// <typeparam name="T">The Enumeration type to retrieve an instance for.</typeparam>
    /// <param name="value">The string value of the Enumeration instance to retrieve.</param>
    /// <returns>The Enumeration instance with the specified string value.</returns>
    /// <exception cref="ApplicationException">Thrown when no Enumeration instance with the specified string value is found.</exception>
    public static T FromValue<T>(string value) where T : Enumeration, new()
    {
        var matchingItem = Parse<T, string>(value, "value", item => string.Equals(item.Value, value, StringComparison.InvariantCulture));
        return matchingItem;
    }

    /// <summary>
    /// Parses an instance of the specified Enumeration type based on a given value and a predicate.
    /// </summary>
    /// <typeparam name="T">The Enumeration type to retrieve an instance for.</typeparam>
    /// <typeparam name="TK">The type of the value to match against.</typeparam>
    /// <param name="value">The value to match against the Enumeration instances.</param>
    /// <param name="description">A description of the value being matched (e.g., "id", "value").</param>
    /// <param name="predicate">A function that determines whether a given Enumeration instance matches the specified value.</param>
    /// <returns>The Enumeration instance that matches the specified value.</returns>
    /// <exception cref="ApplicationException">Thrown when no Enumeration instance matches the specified value.</exception>
    private static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate) where T : Enumeration, new()
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem != null) return matchingItem;
        
        var message = $"'{value}' is not a valid {description} in {typeof(T)}";
        throw new ApplicationException(message);
    }

    /// <summary>
    /// Compares the current Enumeration instance with another object of the same type and returns an integer that indicates their relative order.
    /// </summary>
    /// <param name="other">An object to compare with this instance. Must be of type <see cref="Enumeration"/>.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared. The return value has the following meanings:
    /// <list type="table">
    /// <item>
    /// <term>Less than zero</term>
    /// <description>This instance precedes <paramref name="other"/> in the sort order.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description>This instance follows <paramref name="other"/> in the sort order.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="other"/> is not of type <see cref="Enumeration"/>.</exception>
    public int CompareTo(object? other)
    {
        if (other is not Enumeration otherEnumeration)
        {
            throw new ArgumentException($"Object must be of type {nameof(Enumeration)}", nameof(other));
        }

        return Key.CompareTo(otherEnumeration.Key);
    }
}