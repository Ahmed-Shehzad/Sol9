namespace BuildingBlocks.Extensions.Types;

public static class EnumerableExtensions
{
    /// <summary>
    ///  Creates a difference result of two lists (may have different types).
    /// </summary>
    /// <param name="source">Source list</param>
    /// <param name="destination">Destination list</param>
    /// <param name="comparer">Compare function</param>
    /// <typeparam name="T1">Type of source</typeparam>
    /// <typeparam name="T2">Type of destination</typeparam>
    /// <returns>(down, up, delta)</returns>
    /// <exception cref="ArgumentException"></exception>
    public static (ICollection<T1>, ICollection<T2>, ICollection<(T1, T2)>) Diff<T1, T2>(this IEnumerable<T1> source, 
        IEnumerable<T2> destination, 
        Func<T1, T2, bool> comparer)
    {
        var up = new List<T2>(destination);
        var down = new List<T1>();
        var intersect = new List<(T1, T2)>();

        foreach (var x in source)
        {
            var y = up.SingleOrDefault(s => comparer(x, s));
            if (y is not null)
            {
                intersect.Add((x, y));
                up.Remove(y);
            }
            else
            {
                down.Add(x);
            }
        }
        return (down, up, intersect);
    }
}