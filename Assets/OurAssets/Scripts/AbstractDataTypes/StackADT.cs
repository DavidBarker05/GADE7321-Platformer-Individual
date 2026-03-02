/// <summary>
/// A "last in first out" data structure
/// </summary>
/// <typeparam name="T">The type of data stored in the stack</typeparam>
public class StackADT<T>
{
    System.Collections.Generic.List<T> lifo = new System.Collections.Generic.List<T>();

    /// <summary>
    /// The number of items in the stack
    /// </summary>
    public int Count => lifo.Count;

    /// <summary>
    /// Indicates whether or not the stack has items or not
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Add an item to the back of the stack
    /// </summary>
    /// <param name="item">The item to add to the stack</param>
    public void Push(T item) => lifo.Add(item);

    /// <summary>
    /// Get the item at the back of the stack and remove it from the stack
    /// </summary>
    /// <returns>The item at the back of the stack</returns>
    /// <exception cref="EmptyStackException">Thrown if the stack has no items</exception>
    public T Pop()
    {
        if (IsEmpty) throw new EmptyStackException("The stack has no items to access");
        T item = lifo[lifo.Count - 1];
        lifo.RemoveAt(lifo.Count - 1);
        return item;
    }

    /// <summary>
    /// Get the item at the back of the stack
    /// </summary>
    /// <returns>The item at the back of the stack</returns>
    /// <exception cref="EmptyStackException">Thrown if the stack has no items</exception>
    public T Peek()
    {
        if (IsEmpty) throw new EmptyStackException("The stack has no items to access");
        return lifo[lifo.Count - 1];
    }

    /// <summary>
    /// Clears all items in the stack
    /// </summary>
    public void Clear() => lifo.Clear();
}

/// <summary>
/// Exception thrown when trying to access or remove an item from an empty stack
/// </summary>
public class EmptyStackException : System.Exception
{
    public EmptyStackException() { }

    public EmptyStackException(string message) : base(message) { }

    public EmptyStackException(string message, System.Exception inner) : base(message, inner) { }
}
