using System.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BuildingBlocks.Extensions.Types;

public static partial class JsonElementExtension
{
    /// <summary>
    /// Retrieves the value of a JSON property from the given <see cref="JsonElement"/>, handling various data types.
    /// </summary>
    /// <param name="property">The JSON element to retrieve the value from.</param>
    /// <param name="type">The type of the value to retrieve. If null, the function will return null.</param>
    /// <returns>
    /// The value of the property as the specified type, or null if the specified type is nullable and the property value is null or undefined.
    /// Throws a <see cref="NotSupportedException"/> if the operation was not successful or the property does not exist in the object.
    /// </returns>
    private static object? GetValue(JsonElement property, Type? type)
    {
        switch (property.ValueKind)
        {
            case JsonValueKind.Null or JsonValueKind.Undefined when type.IsNullable():
                return null;
            case JsonValueKind.Null or JsonValueKind.Undefined:
                throw new NotSupportedException();
            case JsonValueKind.True:
            case JsonValueKind.False:
            {
                if (type == typeof(bool))
                {
                    return property.GetBoolean();
                }
                throw new NotSupportedException();
            }
            case JsonValueKind.Number when type == typeof(int):
                return property.GetInt32();
            case JsonValueKind.Number when type == typeof(decimal):
                return property.GetDecimal();
            case JsonValueKind.Number when type == typeof(double):
                return property.GetDouble();
            case JsonValueKind.Number when type == typeof(byte):
                return property.GetByte();
            case JsonValueKind.Number when type == typeof(short):
                return property.GetInt16();
            case JsonValueKind.Number when type == typeof(long):
                return property.GetInt64();
            case JsonValueKind.Number:
                throw new NotSupportedException();
            case JsonValueKind.String when type == typeof(DateTime):
                return property.GetDateTime();
            case JsonValueKind.String when type == typeof(string):
                return property.GetString();
            case JsonValueKind.String:
                throw new NotSupportedException();
            default:
                throw new NotSupportedException();
        }
    }
       
    /// <summary>
    /// Retrieves an array value from the given JSON element, handling various data types.
    /// </summary>
    /// <param name="property">The JSON element to retrieve the array value from.</param>
    /// <param name="elementType">The type of the elements in the resulting array. If null, the function will return null.</param>
    /// <returns>
    /// An array of the specified type containing the elements from the given JSON element.
    /// If the specified type is null, the function will return null.
    /// Throws a <see cref="NotSupportedException"/> if the operation was not successful or the property does not exist in the object.
    /// </returns>
    private static object[]? GetArrayValue(JsonElement property, Type? elementType)
    {
        switch (property.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            case JsonValueKind.Array:
                return property
                    .EnumerateArray()
                    .Select(p => GetValue(p, elementType))
                    .ToArray(elementType);
            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts the given enumerable to an array of the specified type.
    /// </summary>
    /// <param name="source">The enumerable to convert to an array.</param>
    /// <param name="type">The type of the elements in the resulting array. If null, the function will return null.</param>
    /// <returns>
    /// An array of the specified type containing the elements from the given enumerable.
    /// If the specified type is null, the function will return null.
    /// </returns>
    private static object[]? ToArray(this IEnumerable source, Type? type)
    {
        if (type is null) return null;
        
        var enumerable = source as object[] ?? source.Cast<object>().ToArray();
        var arr = (object[])Array.CreateInstance(type, enumerable.Length);
        enumerable.CopyTo(arr, 0);
        return arr;
    }
       
    /// <summary>
    /// Retrieves a JSON object member with the specified property name from the given <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="property">The <see cref="JsonElement"/> to retrieve the object member from.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>
    /// The JSON object member with the specified property name if the operation was successful and the property exists in the object.
    /// Throws a <see cref="NotSupportedException"/> if the operation was not successful or the property does not exist in the object.
    /// </returns>
    private static JsonElement GetJsonObjectMember(JsonElement property, string propertyName)
    {
        if (property.ValueKind == JsonValueKind.Object && property.TryGetProperty(propertyName, out var element))
        {
            return element;
        }
        throw new NotSupportedException();
    }

    /// <summary>
    /// Attempts to retrieve a JSON array element at the specified index from the given <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="property">The <see cref="JsonElement"/> to retrieve the array element from.</param>
    /// <param name="index">The index of the array element to retrieve.</param>
    /// <returns>
    /// The JSON array element at the specified index if the operation was successful and the specified index exists in the array.
    /// Throws a <see cref="NotSupportedException"/> if the operation was not successful or the specified index does not exist in the array.
    /// </returns>
    private static JsonElement GetJsonArrayMember(JsonElement property, int index)
    {
        if (property.ValueKind == JsonValueKind.Array && property.TryGetIndex(index, out var element))
        {
            return element;
        }
        throw new NotSupportedException();
    }
        
    private static readonly Regex PathSeparator = PathSeparatorRegex();
        
    /// <summary>
    /// Checks if the given property name represents a property path with array indices or nested properties.
    /// </summary>
    /// <param name="property">The property name to check.</param>
    /// <param name="parts">When this method returns, contains the individual parts of the property path, 
    /// if the operation was successful; otherwise, contains an empty array. 
    /// The parameter is passed uninitialized.</param>
    /// <returns>
    /// <see langword="true"/> if the property name represents a property path; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsPropertyPath(string property, out string[] parts)
    {
        parts = PathSeparator
            .Split(property)
            .Where(str => !string.IsNullOrEmpty(str))
            .ToArray();
        
        if (parts.Length > 1) return true;
        parts = [];
        return false;
    }
        
    /// <summary>
    /// Attempts to retrieve a JSON array element at the specified index from the given <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> to retrieve the array element from.</param>
    /// <param name="index">The index of the array element to retrieve.</param>
    /// <param name="value">When this method returns, contains the JSON array element at the specified index, 
    /// if the operation was successful; otherwise, contains the default value for the type of the <paramref name="value"/> parameter. 
    /// The parameter is passed uninitialized.</param>
    /// <returns>
    /// <see langword="true"/> if the operation was successful and the specified index exists in the array; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetIndex(this JsonElement element, int index, out JsonElement value)
    {
        var result = element
            .EnumerateArray()
            .Select((e, i) => new
            {
                Value = e, Index = i
            })
            .FirstOrDefault(e => e.Index == index);
        if (result != null)
        {
            value = result.Value;
            return true;
        }
        value = default;
        return false;
    }
        
    /// <summary>
    /// Retrieves the value of a JSON property from the given <see cref="JsonElement"/>, 
    /// handling various data types and property paths.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> to retrieve the property from. 
    /// If null, the function will return the default value for type T.</param>
    /// <param name="propertyName">The name of the property to retrieve. 
    /// If the property name contains array indices or nested properties, 
    /// it can be specified using dot notation or JSON path syntax.</param>
    /// <param name="type">The type of the value to retrieve.</param>
    /// <returns>The value of the property as type T, or the default value for type T if the property is not found.
    /// If the property is an array, the function will return an array of type T.
    /// If the property is a nested object, the function will recursively retrieve the value.
    /// If the property is not found, the function will throw a <see cref="NotSupportedException"/>.</returns>
    private static object? Get(this JsonElement element, string propertyName, Type type)
    {
        if (IsPropertyPath(propertyName, out var parts))
        {
            foreach (var part in parts.Take(parts.Length - 1))
            {
                if (int.TryParse(part, out var index))
                {
                    element = GetJsonArrayMember(element, index);
                    continue;
                }
                element = GetJsonObjectMember(element, part);
            }
            propertyName = parts[^1];
        }
        if (element.ValueKind == JsonValueKind.Object && 
            element.TryGetProperty(propertyName, out var property))
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return GetArrayValue(property, elementType);
            }
            if (type.IsValueType || type == typeof(string))
            {
                return GetValue(property, type);
            }
        }
        if (element.ValueKind == JsonValueKind.Array && 
            int.TryParse(propertyName, out var index1) && 
            element.TryGetIndex(index1, out var property1))
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return GetArrayValue(property1, elementType);
            }
            if (type.IsValueType || type == typeof(string))
            {
                return GetValue(property1, type);
            }
        }
        throw new NotSupportedException();
    }

    /// <summary>
    /// Retrieves the value of a JSON property from the given <see cref="JsonElement"/>, 
    /// handling various data types and property paths.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="element">The <see cref="JsonElement"/> to retrieve the property from. 
    /// If null, the function will return the default value for type T.</param>
    /// <param name="propertyName">The name of the property to retrieve. 
    /// If the property name contains array indices or nested properties, 
    /// it can be specified using dot notation or JSON path syntax.</param>
    /// <returns>The value of the property as type T, or the default value for type T if the property is not found.</returns>
    public static T? Get<T>(this JsonElement element, string propertyName)
    {
        return (T?)Get(element, propertyName, typeof(T));
    }
        
    /// <summary>
    /// Retrieves the value of a JSON property from the given <see cref="JsonElement"/>, 
    /// handling various data types and property paths.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="element">The <see cref="JsonElement"/> to retrieve the property from. 
    /// If null, the function will return the default value for type T.</param>
    /// <param name="propertyName">The name of the property to retrieve. 
    /// If the property name contains array indices or nested properties, 
    /// it can be specified using dot notation or JSON path syntax.</param>
    /// <returns>The value of the property as type T, or the default value for type T if the property is not found.</returns>
    public static T? Get<T>(this JsonElement? element, string propertyName)
    {
        return element.HasValue ? Get<T>(element.Value, propertyName) : default;
    }

    [GeneratedRegex(@"\[|\].?|\.")]
    private static partial Regex PathSeparatorRegex();
}