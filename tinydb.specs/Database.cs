using Bogus;
using TinyDb;
using TinyDb.Library.Interfaces;
using Xunit;

public class Entity : IWithId, IEquatable<Entity>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public DateTime JoinedDate { get; set; }

    public bool Equals(Entity? other)
    {
        if (other == null)
            return false;
        return Id == other.Id;
    }

    public override string ToString()
    {
        return $"{Name} ({Email}), #{Id}";
    }
}

public class DatabaseTests : IAsyncLifetime
{
    readonly int _databaseSize = 1000;
    Entity _entityToSearchFor;
    Entity _alternateEntityToSearchFor;
    Database<Entity> _database;
    Faker<Entity> _fakeEntityGenerator;

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        File.Delete("db-test.db");
    }

    public async Task InitializeAsync()
    {
        _fakeEntityGenerator = new();

        _fakeEntityGenerator
            .RuleFor(e => e.Name, fake => fake.Person.FullName)
            .RuleFor(e => e.DateOfBirth, fake => fake.Person.DateOfBirth)
            .RuleFor(e => e.Email, fake => fake.Person.Email)
            .RuleFor(e => e.JoinedDate, fake => fake.Date.Between(DateTime.Today.AddYears(-1), DateTime.Today));
        if (!File.Exists("db.db"))
        {
            _database = new("db.db");
            List<Entity> entities = _fakeEntityGenerator.Generate(_databaseSize);
            for (int i = 0; i < entities.Count; i++)
            {
                await _database.InsertAsync(entities[i]);
            }
            await _database.DisposeAsync();

        }
        File.Copy("db.db", "db-test.db");
        _database = new("db-test.db");
        _entityToSearchFor = await _database.ByIdAsync(1) ?? throw new NullReferenceException("No entities in database?");
        _alternateEntityToSearchFor = await _database.ByIdAsync(_databaseSize) ?? throw new NullReferenceException("No entities in database?");
    }

    [Fact]
    public async Task AllAsync_Should_ReturnAllEntitiesInTheDatabase()
    {
        Entity[] entities = await _database.AllAsync();
        Assert.Equal(_databaseSize, entities.Length);
    }

    [Fact]
    public async Task AllAsync_Should_ReturnEmptyArrayWhenNoEntitiesInTheDatabase()
    {
        Entity[] entities = await _database.AllAsync();
        Assert.Equal(_databaseSize, entities.Length);

        for (int i = entities.Length - 1; i >= 0; i--)
        {
            Entity entity = entities[i];
            await _database.DeleteAsync(entity);
        }

        entities = await _database.AllAsync();
        Assert.Empty(entities);
    }

    [Fact]
    public async Task ByIdAsync_Should_ReturnEntityWhenFoundInDatabase()
    {
        Entity? foundEntity = await _database.ByIdAsync(_entityToSearchFor.Id);
        Assert.NotNull(foundEntity);
        Assert.Equal(_entityToSearchFor, foundEntity);

        Entity? alternateFoundEntity = await _database.ByIdAsync(_alternateEntityToSearchFor.Id);
        Assert.NotNull(alternateFoundEntity);
        Assert.Equal(_alternateEntityToSearchFor, alternateFoundEntity);
    }

    [Fact]
    public async Task ByIdAsync_Should_ReturnNullWhenNotFoundInDatabase()
    {
        await _database.DeleteAsync(_entityToSearchFor);
        Entity? foundEntity = await _database.ByIdAsync(_entityToSearchFor.Id);
        Assert.Null(foundEntity);
    }

    [Fact]
    public async Task InsertAsync_Should_ReturnEntityWithPopulatedId()
    {
        Entity entityToInsert = _fakeEntityGenerator.Generate();
        Entity insertedEntity = await _database.InsertAsync(entityToInsert);

        Assert.Equal(_databaseSize + 1, insertedEntity.Id);
        Assert.Equal(entityToInsert.Name, insertedEntity.Name);
        Assert.Equal(entityToInsert.Email, insertedEntity.Email);
        Assert.Equal(entityToInsert.DateOfBirth, insertedEntity.DateOfBirth);
        Assert.Equal(entityToInsert.JoinedDate, insertedEntity.JoinedDate);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnEntityWithUpdatedInfo()
    {
        Entity? entityToUpdate = await _database.ByIdAsync(_alternateEntityToSearchFor.Id);
        Assert.NotNull(entityToUpdate);
        entityToUpdate.JoinedDate = DateTime.Today;
        Entity updatedEntityFromDatabase = await _database.UpdateAsync(entityToUpdate);

        Assert.NotNull(updatedEntityFromDatabase);
        Assert.Equal(DateTime.Today, updatedEntityFromDatabase.JoinedDate);
    }

    [Fact]
    public async Task UpdateAsync_Should_ThrowWhenEntityDoesNotExistInDatabase()
    {
        Entity entityToUpdate = _fakeEntityGenerator.Generate();
        entityToUpdate.Id = _databaseSize + 1;
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _database.UpdateAsync(entityToUpdate));
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveEntityFromDatabase()
    {
        await _database.DeleteAsync(_alternateEntityToSearchFor);
        Entity[] entities = await _database.AllAsync();
        Assert.Equal(_databaseSize - 1, entities.Length);
        Assert.DoesNotContain(_alternateEntityToSearchFor, entities);
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowWhenEntityDoesNotExistInDatabase()
    {
        Entity entityToDelete = _fakeEntityGenerator.Generate();
        entityToDelete.Id = _databaseSize + 1;
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _database.DeleteAsync(entityToDelete));
    }

    [Fact]
    public void InitializingMultipleDatabases_Should_AllowOperationsConcurrently()
    {
        Database<Entity> secondDatabase = new("db-test.db");
        Database<Entity>[] databases = [_database, secondDatabase, new("db-test.db")];
        Parallel.ForEach(databases, async database => await database.AllAsync());
    }
}