/// <summary>
/// A "first in first out" data structure
/// </summary>
/// <typeparam name="T">The type of data stored in the queue</typeparam>
public class QueueADT<T>
{
    System.Collections.Generic.List<T> fifo = new System.Collections.Generic.List<T>();

    /// <summary>
    /// The number of items in the queue
    /// </summary>
    public int Count => fifo.Count;

    /// <summary>
    /// Indicates whether or not the queue has items or not
    /// </summary>
    public bool IsEmpty => Count == 0;
    
    /// <summary>
    /// Add an item to the back of the queue
    /// </summary>
    /// <param name="item">The item to add to the queue</param>
    public void Enqueue(T item) => fifo.Add(item);

    /// <summary>
    /// Get the item at the front of the queue and remove it from the queue
    /// </summary>
    /// <returns>The item at the front of the queue</returns>
    /// <exception cref="EmptyQueueException">Thrown if the queue has no items</exception>
    public T Dequeue()
    {
        if (IsEmpty) throw new EmptyQueueException("The queue has no items to access");
        T item = fifo[0];
        fifo.RemoveAt(0);
        return item;
    }

    /// <summary>
    /// Get the item at the front of the queue
    /// </summary>
    /// <returns>The item at the front of the queue</returns>
    /// <exception cref="EmptyQueueException">Thrown if the queue has no items</exception>
    public T Peek()
    {
        if (IsEmpty) throw new EmptyQueueException("The queue has no items to access");
        return fifo[0];
    }

    /// <summary>
    /// Clears all items in the queue
    /// </summary>
    public void Clear() => fifo.Clear();
}

/// <summary>
/// Exception thrown when trying to access or remove an item from an empty queue
/// </summary>
public class EmptyQueueException : System.Exception
{
    public EmptyQueueException() { }

    public EmptyQueueException(string message) : base(message) { }

    public EmptyQueueException(string message, System.Exception inner) : base(message, inner) { }
}
