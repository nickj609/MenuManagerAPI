// Included libraries
using System.Text;
using System.Collections.Concurrent;

// Declare namespace
namespace MenuManagerAPI.CrossCutting;

// Define static class
public static class StringBuilderPool
{
    // Define class properties
    private const int DefaultCapacity = 512;
    private const int MaxCapacity = 4096;
    private const int MaxPoolSize = 64;
    private static readonly ConcurrentBag<StringBuilder> Pool = new();
    private static int _currentPoolSize = 0;
    public static int Count => _currentPoolSize;

    // Define class methods
    public static StringBuilder Get()
    {
        if (Pool.TryTake(out var builder))
        {
            Interlocked.Decrement(ref _currentPoolSize);
            return builder;
        }

        return new StringBuilder(DefaultCapacity);
    }

    public static void Return(StringBuilder builder)
    {
        if (builder == null)
            return;

        builder.Clear();

        if (builder.Capacity > MaxCapacity)
            builder.Capacity = DefaultCapacity;

        if (_currentPoolSize < MaxPoolSize)
        {
            Pool.Add(builder);
            Interlocked.Increment(ref _currentPoolSize);
        }
    }
    public static void Clear()
    {
        while (Pool.TryTake(out _))
        {
            Interlocked.Decrement(ref _currentPoolSize);
        }
    }
}

// Define static class
public static class StringBuilderPoolExtensions
{

    // Define class methods
    public static string Use(Action<StringBuilder> action)
    {
        var builder = StringBuilderPool.Get();
        try
        {
            action(builder);
            return builder.ToString();
        }
        finally
        {
            StringBuilderPool.Return(builder);
        }
    }

    public static T Use<T>(Func<StringBuilder, T> func)
    {
        var builder = StringBuilderPool.Get();
        try
        {
            return func(builder);
        }
        finally
        {
            StringBuilderPool.Return(builder);
        }
    }
}
