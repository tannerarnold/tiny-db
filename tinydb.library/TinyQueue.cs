namespace TinyDb.Library;

/// <summary>
/// A first-in, first-out (FIFO) linked list. Insertion, removal, and access is all O(1).
/// </summary>
/// <typeparam name="T">The type of the values stored in the queue</typeparam>
public class TinyQueue<T>
{

    /// <summary>
    /// The front of the queue (i.e. where items are removed / dequeued)
    /// </summary>
    private Node? _head;
    /// <summary>
    /// The back of the queue (i.e. where items are added / enqueued)
    /// </summary>
    private Node? _tail;
    /// <summary>
    /// The length of the queue
    /// </summary>
    public int Length { get; private set; }
    /// <summary>
    /// Is the queue empty?
    /// </summary>
    public bool IsEmpty => Length == 0;

    /// <summary>
    /// A node in a queue (essentially a linked list with restrictions on where to add and remove from the queue)
    /// </summary>
    /// <param name="value"></param>
    private class Node(T value)
    {
        /// <summary>
        /// The next node in the queue sequence
        /// </summary>
        public Node? Next { get; set; }
        /// <summary>
        /// The value stored in this specific spot in the queue
        /// </summary>
        public T Value { get; set; } = value;
    }

    /// <summary>
    /// Add a new value to the back of the queue.
    /// </summary>
    /// <param name="value">The value to add to the back of the queue</param>
    public void Enqueue(T value)
    {
        Node node = new(value);
        Length++;
        // Generally, if the head and tail are null, that means we have an empty queue.
        // Populate the head and the tail with the only element in the queue.
        if (_head == null || _tail == null)
        {
            _head = node;
            _tail = node;
        }
        // Otherwise, this goes on the back of the queue.
        else
        {
            _tail.Next = node;
            _tail = node;
        }

    }

    /// <summary>
    /// Utility method to get the current head node, and throw if it does not exist.
    /// </summary>
    /// <returns>The front node of the queue</returns>
    /// <exception cref="InvalidOperationException">The front node is not present, which means the queue is empty</exception>
    private Node CurrentHead()
    {
        return _head ?? throw new InvalidOperationException("Cannot get value when there are no items in the queue!");
    }

    /// <summary>
    /// Remove a value from the front of the queue.
    /// </summary>
    /// <returns>The value from the front of the queue</returns>
    /// <exception cref="InvalidOperationException">The queue is empty, and cannot return a value</exception>
    public T Dequeue()
    {
        Node node = CurrentHead();
        Length--;
        // Set the head to the next in line.
        _head = node.Next;
        // If the length is zero, we assume that the queue is now empty, and the head was set as null.
        // Therefore, we need to set the tail as empty, as there is no longer any items in the queue.
        if (Length == 0)
        {
            _tail = null;
        }
        return node.Value;
    }

    /// <summary>
    /// View the value at the front of the queue without removing it from the queue.
    /// </summary>
    /// <returns>The value from the front of the queue</returns>
    /// <exception cref="InvalidOperationException">The queue is empty, and cannot return a value</exception>
    public T Peek()
    {
        Node? node = CurrentHead();
        return node.Value;
    }

    /// <summary>
    /// Formats the queue in string form, formatted as [values].
    /// Front to back is represented from left to right.
    /// Nulls are represented as "null".
    /// </summary>
    /// <returns>A string representation of the queue</returns>
    public override string ToString()
    {
        string repr = "[";
        Node? currentNode = _head;
        for (int i = 0; i < Length; i++)
        {
            repr += currentNode?.Value?.ToString() ?? "null";
            if (i < Length - 1)
            {
                repr += ", ";
            }
            currentNode = currentNode?.Next;
        }
        return repr + "]";
    }
}