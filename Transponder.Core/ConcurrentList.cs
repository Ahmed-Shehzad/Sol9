using System.Collections.ObjectModel;

namespace Transponder.Core;

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
    
    public void RemoveAll(IEnumerable<T> items)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                _list.Remove(item);
            }
        }
    }
    
    public ConcurrentList<T> Where(Func<T, bool> predicate)
    {
        lock (_lock)
        {
            var list = new ConcurrentList<T>();
            list.AddRange(_list.Where(predicate));
            return list;
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