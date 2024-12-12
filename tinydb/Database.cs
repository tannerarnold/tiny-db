using TinyDb.Library.Interfaces;
using TinyDb.Library;
using System.Text;

namespace TinyDb;

/// <summary>
/// This class represents the connection to your single file database.
/// It was designed to have dead simple and rapid operations for creating, reading, updating, and deleting.
/// It is capable of storing anything that can be serialized to JSON.
/// </summary>
/// <remarks>
/// The database must be created using a "await using" statement, which should manage reading and saving data from the file.
/// Currently, it does not handle complex operations, like filtering, or searching by parameters other than the Id.
/// Entities need to extend TinyDb.Library.Interfaces.IWithId in order to work with this database.
/// </remarks>
/// <typeparam name="T">The type you are storing in the database</typeparam>
public class Database<T> : IAsyncDisposable where T : IWithId
{
    /// <summary>
    /// The next ID in the database sequence that new entities end up with.
    /// </summary>
    private int _nextId;
    /// <summary>
    /// Lock on the database file when the file is being used. Only will allow one thread at a time
    /// to access the database. This is to prevent race conditions, deadlocks, and inconsistent data.
    /// </summary>
    private readonly SemaphoreSlim semaphore = new(1);
    /// <summary>
    /// The up to date, current list of rows in memory. When the database closes,
    /// this will be serialized into the database file.
    /// </summary>
    private TinyRowList<T> _currentRows;
    /// <summary>
    /// The path in which the database is stored at / read from.
    /// </summary>
    private readonly string _path;

    /// <summary>
    /// Create a new instance of the database.
    /// </summary>
    /// <param name="path">The path in which the database should be stored / read from</param>
    public Database(string path)
    {
        _path = path;
        Load();
    }

    /// <summary>
    /// Retrieve an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the desired entity to retrieve</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Entity if it is found, null otherwise</returns>
    public async Task<T?> ByIdAsync(int id, CancellationToken token = default)
    {
        await semaphore.WaitAsync(token);
        T? entity = _currentRows.Find(id);
        semaphore.Release();
        return entity;
    }

    /// <summary>
    /// Retrieves every record from the database asynchronously.
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>An array of every entity in the database</returns>
    public async Task<T[]> AllAsync(CancellationToken token = default)
    {
        await semaphore.WaitAsync(token);
        T[] entities = _currentRows.ToArray();
        semaphore.Release();
        return entities;
    }

    /// <summary>
    /// Insert a new entity into the database asynchronously.
    /// The ID field on the entity will be populated with the new ID of the entity.
    /// </summary>
    /// <param name="entity">The entity to be inserted</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The newly inserted entity with an ID populated</returns>
    public async Task<T> InsertAsync(T entity, CancellationToken token = default)
    {
        await semaphore.WaitAsync(token);
        int newId = _nextId;
        entity.Id = newId;
        _currentRows.Insert(entity);
        semaphore.Release();
        _nextId++;
        await SaveAsync(token);
        await LoadAsync(token);
        return entity;
    }

    /// <summary>
    /// Updates the entity passed in in the database with the specified ID on the entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be updated</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>The updated entity from the database</returns>
    public async Task<T> UpdateAsync(T entity, CancellationToken token = default)
    {
        await semaphore.WaitAsync(token);
        try
        {
            // Remove can throw if the entity with the ID is not found
            _currentRows.Remove(entity);
            _currentRows.Insert(entity);
            _currentRows.Sort();
            semaphore.Release();
            await SaveAsync(token);
            await LoadAsync(token);
            return entity;
        }
        catch (Exception)
        {
            // Always release the semaphore
            semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Delete an entity from the database asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public async Task DeleteAsync(T entity, CancellationToken token = default)
    {
        await semaphore.WaitAsync(token);
        try
        {
            // Remove can throw if the entity with the ID is not found
            _currentRows.Remove(entity);
            semaphore.Release();
            await SaveAsync(token);
            await LoadAsync(token);
        }
        catch (Exception)
        {
            // Always release the semaphore
            semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Saves the changes to the file asynchronously.
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    private async Task SaveAsync(CancellationToken token = default)
    {
        await semaphore.WaitAsync(token);
        // Null out the file, as BinaryWriter will only write the size of the array
        // Caused some headaches when parsing back invalid / partial JSON
        File.WriteAllBytes(_path, []);
        using FileStream stream = File.Open(_path, FileMode.Open, FileAccess.Write, FileShare.Write);
        using BinaryWriter writer = new(stream, Encoding.UTF8, false);
        writer.Write(_nextId);
        writer.Write(_currentRows.ToBytes());
        semaphore.Release();
    }

    /// <summary>
    /// Called by the constructor on initial load.
    /// Responsible for creating the file and providing sane defaults.
    /// If the file is already created, it reads in the information.
    /// </summary>
    private void Load()
    {
        semaphore.Wait();
        if (!File.Exists(_path))
        {
            File.Create(_path);
        }
        FileInfo info = new(_path);
        if (info.Length == 0)
        {
            _currentRows = new TinyRowList<T>(100);
            _nextId = 1;
        }
        else
        {
            using FileStream stream = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using BinaryReader reader = new(stream, Encoding.UTF8, false);
            _nextId = reader.ReadInt32();
            _currentRows = TinyRowList<T>.FromBytes(reader.ReadBytes(Convert.ToInt32(info.Length) - sizeof(int)));
        }
        semaphore.Release();
    }

    /// <summary>
    /// Asynchronously load back in all the persisted information.
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    private async Task LoadAsync(CancellationToken token)
    {
        await semaphore.WaitAsync(token);
        FileInfo info = new(_path);
        using FileStream stream = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        using BinaryReader reader = new(stream, Encoding.UTF8, false);
        _nextId = reader.ReadInt32();
        _currentRows = TinyRowList<T>.FromBytes(reader.ReadBytes(Convert.ToInt32(info.Length) - sizeof(int)));
        semaphore.Release();
    }

    /// <summary>
    /// Close the connection, and persist all changes.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await SaveAsync();
    }
}