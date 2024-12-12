using BenchmarkDotNet.Attributes;
using TinyDb;
using TinyDb.Library.Interfaces;

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

public class DatabaseBenchmarks
{

    private Database<Entity> _database;

    [Params(100, 1000, 10000)]
    public int DatabaseSize { get; set; }

    [IterationSetup]
    public async void Setup()
    {
        if (!File.Exists("db.db"))
        {
            _database = new("db.db");
            await _database.DisposeAsync();
        }
        File.Copy("db.db", "db-test.db");
        _database = new("db-test.db");
        for (int i = 0; i < DatabaseSize; i++)
        {
            Entity entity = new(i + 1);
            await _database.InsertAsync(entity);
        }
    }

    [IterationCleanup]
    public async void Teardown()
    {
        await _database.DisposeAsync();
        File.Delete("db-test.db");
    }

    [Benchmark]
    public Task<Entity[]> AllAsync() => _database.AllAsync();

    [Benchmark]
    public Task<Entity?> ByIdAsync() => _database.ByIdAsync(50);

    [Benchmark]
    public Task<Entity> InsertAsync() => _database.InsertAsync(new Entity(DatabaseSize + 1));

    [Benchmark]
    public Task<Entity> UpdateAsync() => _database.UpdateAsync(new Entity(50));

    [Benchmark]
    public Task DeleteAsync() => _database.DeleteAsync(new Entity(50));
}