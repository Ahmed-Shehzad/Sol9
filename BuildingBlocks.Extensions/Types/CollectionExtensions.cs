namespace BuildingBlocks.Extensions.Types;

public static class CollectionExtensions
{
    /// <summary>
    ///  Removes a collection of items from a list.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="itemsToRemove"></param>
    /// <typeparam name="T"></typeparam>
    public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> itemsToRemove)
    {
        foreach (var item in itemsToRemove)
        {
            collection.Remove(item);
        }
    }
        
    /// <summary>
    ///  Adds a collection of items to a list
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="itemsToAdd"></param>
    /// <typeparam name="T"></typeparam>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> itemsToAdd)
    {
        foreach (var item in itemsToAdd)
        {
            collection.Add(item);
        }
    }
}