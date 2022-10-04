using System;

namespace Checkers.Logic;

public class Cached<T>
{
    private readonly Func<T> _factory;
    private Lazy<T> _cached;
    public T Value => _cached.Value;

    public Cached(Func<T> factory)
    {
        _factory = factory;
        _cached = new Lazy<T>(factory);
    }

    public void Reset()
    {
        _cached = new Lazy<T>(_factory);
    }
}
