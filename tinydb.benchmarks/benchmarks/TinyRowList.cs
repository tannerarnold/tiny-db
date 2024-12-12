using BenchmarkDotNet.Attributes;
using TinyDb.Library;
using TinyDb.Library.Interfaces;

namespace TinyDb.Benchmarks;

public class Entity(int id) : IWithId, IEquatable<Entity>
{
    public int Id { get; set; } = id;

    public bool Equals(Entity? other)
    {
        if (other == null)
            return false;
        return Id.Equals(other.Id);
    }

    public override string ToString()
    {
        return Id.ToString();
    }
}

public class TinyRowListBenchmarks
{
    private readonly int _length = 10000;

    private TinyRowList<Entity> _list;

    [IterationSetup]
    public void Setup()
    {
        _list = new TinyRowList<Entity>(_length);
        for (int i = 1; i <= _length; i++)
        {
            _list.Insert(new Entity(i));
        }
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _list = new TinyRowList<Entity>(_length);
    }

    [Benchmark]
    public void Insert() => _list.Insert(new Entity(0));

    [Benchmark]
    public void InsertAtStart() => _list.InsertAt(1000, new Entity(0));

    [Benchmark]
    public void InsertAtMiddle() => _list.InsertAt(5000, new Entity(0));

    [Benchmark]
    public void InsertAtEnd() => _list.InsertAt(9000, new Entity(0));

    [Benchmark]
    public Entity GetAtStart() => _list[1000];

    [Benchmark]
    public Entity GetAtMiddle() => _list[5000];

    [Benchmark]
    public Entity GetAtEnd() => _list[9000];

    [Benchmark]
    public void SetAtStart() => _list[1000] = new Entity(0);

    [Benchmark]
    public void SetAtMiddle() => _list[5000] = new Entity(0);

    [Benchmark]
    public void SetAtEnd() => _list[9000] = new Entity(0);

    [Benchmark]
    public void RemoveAtStart() => _list.RemoveAt(1000);

    [Benchmark]
    public void RemoveAtMiddle() => _list.RemoveAt(5000);

    [Benchmark]
    public void RemoveAtEnd() => _list.RemoveAt(9000);

    [Benchmark]
    public TinyRowList<Entity> Sort()
    {
        _list.Insert(new Entity(500));
        return _list.Sort();
    }

    [Benchmark]
    public Entity? FindAtStart() => _list.Find(1000);

    [Benchmark]
    public Entity? FindAtMiddle() => _list.Find(5000);

    [Benchmark]
    public Entity? FindAtEnd() => _list.Find(9000);
}