using System;
using System.Collections;
using System.Collections.Generic;

public class StackADT<T> : IReadOnlyCollection<T>
{
    public bool IsEmpty => Count == 0;

    public int Count { get; private set; }

    public T Top
    {
        get
        {
            if (IsEmpty) throw new EmptyStackException("The stack has no items to access");
            return m_Array[Count - 1];
        }
    }

    public StackADT() : this(0) { }

    public StackADT(int capacity)
    {
        if (capacity < 0) throw new ArgumentException("capacity needs to be greater than or equal to 0");
        m_Array = capacity > 0 ? new T[capacity] : Array.Empty<T>();
        Count = 0;
        m_Comparer = EqualityComparer<T>.Default;
    }

    public StackADT(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        m_Array = new T[1];
        Count = 0;
        m_Comparer = EqualityComparer<T>.Default;
        foreach (T item in collection) Push(item);
    }

    public void Push(T item)
    {
        if (m_Array.Length == 0) m_Array = new T[1];
        else if (Count == m_Array.Length) Resize(m_Array.Length * 2);
        m_Array[Count++] = item;
    }

    public T Pop()
    {
        if (IsEmpty) throw new EmptyStackException("The stack has no items to access");
        T item = m_Array[--Count];
        m_Array[Count] = default;
        if (Count < m_Array.Length / 4) Resize(m_Array.Length / 2);
        return item;
    }

    public void Clear()
    {
        m_Array = Array.Empty<T>();
        Count = 0;
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (m_Comparer.Equals(m_Array[i], item)) return true;
        }
        return false;
    }

    public T[] ToArray()
    {
        T[] _array = new T[Count];
        CopyTo(_array, 0);
        return _array;
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void CopyTo(T[] array, int index)
    {
        if (array == null) throw new ArgumentNullException("array");
        if (index < 0 || index > array.Length) throw new IndexOutOfRangeException("arrayIndex out of range");
        if (array.Length - index < Count) throw new ArgumentException("not enough space for the Stack in array");
        for (int i = Count - 1; i >= 0; --i)
        {
            array[index++] = m_Array[i];
        }
    }

    void Resize(int newCapacity)
    {
        if (newCapacity > 0) Array.Resize(ref m_Array, newCapacity);
        else m_Array = Array.Empty<T>();
    }

    T[] m_Array;
    IEqualityComparer<T> m_Comparer;

    public struct Enumerator : IEnumerator<T>, IEnumerator
    {
        public T Current => m_CurrentValue;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (m_Index < 0) return false;
            m_CurrentValue = m_Stack.m_Array[m_Index--];
            return true;
        }

        public void Reset()
        {
            m_CurrentValue = default;
            m_Index = m_Stack.Count - 1;
        }

        internal Enumerator(StackADT<T> stack)
        {
            m_Stack = stack;
            m_CurrentValue = default;
            m_Index = m_Stack.Count - 1;
        }

        StackADT<T> m_Stack;
        T m_CurrentValue;
        int m_Index;
    }
}

public class EmptyStackException : Exception
{
    public EmptyStackException() { }

    public EmptyStackException(string message) : base(message) { }

    public EmptyStackException(string message, Exception inner) : base(message, inner) { }
}
