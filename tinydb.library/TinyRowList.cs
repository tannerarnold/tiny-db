using System.Text;
using System.Text.Json;
using TinyDb.Library.Interfaces;

namespace TinyDb.Library;

/// <summary>
/// Which way should you sort when running a sort operation on the list?
/// </summary>
public enum SortOrder
{
    Ascending,
    Descending
}

/// <summary>
/// An array list implementation. Allows operations to occur at O(1) for access, setting, and insert at end.
/// Removal, removal at a certain index, and insertion at a certain index, are O(N).
/// Find is O(logN), while sorting currently is O(N*N).
/// </summary>
/// <typeparam name="T">The type of the values stored in the list</typeparam>
public class TinyRowList<T> where T : IWithId
{
    /// <summary>
    /// Public constructor for a list.
    /// </summary>
    /// <param name="initialCapacity">How many items should the backing array be?</param>
    public TinyRowList(int initialCapacity)
    {
        _capacity = initialCapacity;
        _items = new T[initialCapacity];
    }

    /// <summary>
    /// Private constructor for reconstructing a list from disk.
    /// </summary>
    /// <param name="initialCapacity">How many items should the backing array be?</param>
    /// <param name="items">The items from disk that should be stored</param>
    private TinyRowList(int initialCapacity, T[] items)
    {
        _capacity = initialCapacity;
        _items = new T[_capacity];
        Length = items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            _items[i] = items[i];
        }
    }

    /// <summary>
    /// How large is the array?
    /// </summary>
    private int _capacity;

    /// <summary>
    /// The backing array for the list.
    /// </summary>
    private T[] _items;

    /// <summary>
    /// How many items are in the list?
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Is the list currently empty?
    /// </summary>
    public bool IsEmpty => Length == 0;

    /// <summary>
    /// Is the index being accessed out of bounds of the list?
    /// </summary>
    /// <param name="index">The index being accessed</param>
    /// <returns></returns>
    private bool IsOutOfBounds(int index) => index < 0 || index > Length - 1;

    /// <summary>
    /// When the array is too small for the list, 
    /// expand the capacity of the array and copy over the original items.
    /// </summary>
    private void ExpandArray()
    {
        T[] copy = _items;
        _capacity *= 2;
        _items = new T[_capacity];
        for (int i = 0; i < copy.Length; i++)
        {
            _items[i] = copy[i];
        }
    }

    /// <summary>
    /// Access the value at a certain index in the list.
    /// </summary>
    /// <param name="index">The index in the list to find said value</param>
    /// <returns>The value</returns>
    /// <exception cref="ArgumentOutOfRangeException">There is no value at this index</exception>
    public T this[int index]
    {
        get
        {
            if (IsOutOfBounds(index))
                throw new ArgumentOutOfRangeException(nameof(index));
            return _items[index];

        }
        set
        {
            if (IsOutOfBounds(index))
                throw new ArgumentOutOfRangeException(nameof(index));
            _items[index] = value;
        }
    }

    /// <summary>
    /// Insert a value at the end of the list.
    /// </summary>
    /// <param name="value">The value to insert</param>
    public void Insert(T value)
    {
        Length++;
        if (Length >= _capacity)
        {
            ExpandArray();
        }
        _items[Length - 1] = value;
    }

    /// <summary>
    /// Insert a value at a specific index, shifting values with an index greater forward.
    /// </summary>
    /// <param name="index">The index in the list to insert at</param>
    /// <param name="value">The value to insert</param>
    /// <exception cref="ArgumentOutOfRangeException">There is no value at this index</exception>
    public void InsertAt(int index, T value)
    {
        if (IsOutOfBounds(index))
            throw new ArgumentOutOfRangeException(nameof(index));
        Length++;
        if (Length >= _capacity)
        {
            ExpandArray();
        }
        for (int i = Length; i >= index; i--)
        {
            _items[i + 1] = _items[i];
        }
        _items[index] = value;
    }

    /// <summary>
    /// Removes a value at a specific index, shifting values with an index greater backwards.
    /// </summary>
    /// <param name="index">The index in the list to remove the value from</param>
    /// <exception cref="ArgumentOutOfRangeException">There is no value at this index</exception>
    public void RemoveAt(int index)
    {
        if (IsOutOfBounds(index))
            throw new ArgumentOutOfRangeException(nameof(index));
        Length--;
        for (int i = index; i < Length + 1; i++)
        {
            _items[i] = _items[i + 1];
        }
    }

    /// <summary>
    /// Remove a specific entity from the list.
    /// </summary>
    /// <param name="entity">The entity to search for</param>
    /// <exception cref="InvalidOperationException">Could not find the entity in the list</exception>
    public void Remove(T entity)
    {
        int id = entity.Id;
        int low = 0;
        int high = Length;
        while (low < high)
        {
            int index = (int)Math.Floor((decimal)low + (high - low) / 2);
            T item = this[index];
            if (item.Id == id)
            {
                RemoveAt(index);
                return;
            }
            else if (item.Id > id)
            {
                high = index;
            }
            else
            {
                low = index + 1;
            }
        }
        throw new InvalidOperationException("Cannot remove if it does not exist in the list!");
    }

    /// <summary>
    /// Sort the array in either ascending or descending order using bubble sort.
    /// </summary>
    /// <param name="sortOrder">Should this array be sorted in ascending or descending order?</param>
    /// <returns>The sorted array</returns>
    public TinyRowList<T> Sort(SortOrder sortOrder = SortOrder.Ascending)
    {
        for (int i = 0; i < Length; i++)
        {
            for (int j = 0; j < Length - 1 - i; j++)
            {
                bool shouldSwap = sortOrder == SortOrder.Ascending ? this[j].Id > this[j + 1].Id : this[j].Id < this[j + 1].Id;
                if (shouldSwap)
                {
                    T temp = this[j];
                    this[j] = this[j + 1];
                    this[j + 1] = temp;
                }
            }
        }
        return this;
    }

    /// <summary>
    /// Search for an entity in the list by its id using binary search.
    /// </summary>
    /// <param name="id">The id of the entity to search for</param>
    /// <returns>The entity if found, otherwise null</returns>
    public T? Find(int id)
    {
        int low = 0;
        int high = Length;
        while (low < high)
        {
            int index = (int)Math.Floor((decimal)low + (high - low) / 2);
            T item = this[index];
            if (item.Id == id)
            {
                return item;
            }
            else if (item.Id > id)
            {
                high = index;
            }
            else
            {
                low = index + 1;
            }
        }
        return default;
    }

    /// <summary>
    /// Formats the list in string form, formatted as [values].
    /// Lowest index to highest index is represented from left to right.
    /// Nulls are represented as "null".
    /// </summary>
    /// <returns>A string representation of the list</returns>
    public override string ToString()
    {
        string repr = "[";
        for (int i = 0; i < Length; i++)
        {
            repr += _items[i]?.ToString() ?? "null";
            if (i < Length - 1)
            {
                repr += ", ";
            }
        }
        return repr + "]";
    }

    /// <summary>
    /// Take the list and convert it into an array.
    /// </summary>
    /// <returns>An array of entities</returns>
    public T[] ToArray()
    {
        T[] items = new T[Length];
        for (int i = 0; i < Length; i++)
        {
            items[i] = _items[i];
        }
        return items;
    }

    /// <summary>
    /// Take the list and convert it to bytes.
    /// </summary>
    /// <returns>A byte array representing the list</returns>
    public byte[] ToBytes()
    {
        T[] items = ToArray();
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, false);
        writer.Write(_capacity);
        writer.Write(Length);
        if (!IsEmpty)
            writer.Write(JsonSerializer.SerializeToUtf8Bytes(items));
        return stream.ToArray();
    }

    /// <summary>
    /// Takes an array of bytes, and converts them into an instance of the list.
    /// </summary>
    /// <param name="bytes">The bytes to convert</param>
    /// <returns>A new list with all the parameters of the old list</returns>
    /// <exception cref="Exception">Could not deserialize the byte array</exception>
    public static TinyRowList<T> FromBytes(byte[] bytes)
    {
        int amountOfBytesToRead = bytes.Length - (sizeof(int) * 2);
        using MemoryStream stream = new(bytes);
        using BinaryReader reader = new(stream, Encoding.UTF8, false);
        int capacity = reader.ReadInt32();
        int arrayLength = reader.ReadInt32();
        if (arrayLength > 0)
        {
            byte[] arrayBytes = reader.ReadBytes(amountOfBytesToRead);
            T[] items = JsonSerializer.Deserialize<T[]>(arrayBytes) ?? throw new Exception("Failed to deserialize from bytes!");
            return new TinyRowList<T>(capacity, items);
        }
        else
        {
            return new TinyRowList<T>(capacity);
        }

    }
}