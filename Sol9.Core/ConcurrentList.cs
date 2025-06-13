using System.Collections.ObjectModel;

namespace Sol9.Core;

public class ConcurrentList<T>
{
    private readonly List<T> _list;
    private readonly object _lock;

    public ConcurrentList()
    {
        _list = [];
        _lock = new object();
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            _list.Add(item);
        }
    }

    public bool Remove(T item)
    {
        lock (_lock)
        {
            return _list.Remove(item);
        }
    }
    
    public ReadOnlyCollection<T> AsReadOnly()
    {
        lock (_lock)
        {
            return _list.AsReadOnly();
        }
    }
    
    public T? Find(Predicate<T> predicate)
    {
        lock (_lock)
        {
            return _list.Find(predicate);
        }
    }
    
    public void AddRange(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _list.AddRange(items);
        }
    }
    
    public T this[int index]
    {
        get
        {
            lock (_lock)
            {
                return _list[index];
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _list.Count;
            }
        }
    }

    public List<T> Snapshot()
    {
        lock (_lock)
        {
            return [.._list];
        }
    }
}