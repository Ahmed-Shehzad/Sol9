using System.Reflection;

namespace BuildingBlocks.Domain.Types;

public abstract class Enumeration(int id, string value) : IComparable
{
    public int Id
    {
        get => id;
    }

    public string Value
    {
        get => value;
    }

    public override string ToString()
    {
        return Value;
    }

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

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
        return absoluteDifference;
    }

    public static T FromId<T>(int id) where T : Enumeration, new()
    {
        var matchingItem = Parse<T, int>(id, "id", item => item.Id == id);
        return matchingItem;
    }

    public static T FromValue<T>(string value) where T : Enumeration, new()
    {
        var matchingItem = Parse<T, string>(value, "value", item => item.Value == value);
        return matchingItem;
    }

    private static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate) where T : Enumeration, new()
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem != null) return matchingItem;
        
        var message = $"'{value}' is not a valid {description} in {typeof(T)}";
        throw new ApplicationException(message);
    }

    public int CompareTo(object? other)
    {
        if (other is not Enumeration otherEnumeration)
        {
            throw new ArgumentException($"Object must be of type {nameof(Enumeration)}", nameof(other));
        }

        return Id.CompareTo(otherEnumeration.Id);
    }
}