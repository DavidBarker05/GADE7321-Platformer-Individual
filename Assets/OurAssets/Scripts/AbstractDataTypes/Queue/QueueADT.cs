using System;
using System.Collections;
using System.Collections.Generic;

public class QueueADT<T> : IReadOnlyCollection<T>
{
    public bool IsEmpty => Count == 0;

    public int Count { get; private set; }

    public T Front
    {
        get
        {
            if (IsEmpty) throw new EmptyQueueException("The queue has no items to access");
            return m_Array[m_Front];
        }
    }

    public QueueADT() : this(0) { }

    public QueueADT(int capacity)
    {
        if (capacity < 0) throw new ArgumentException("capacity needs to be greater than or equal to 0");
        m_Array = new T[capacity];
        Count = 0;
        m_Front = 0;
        m_Comparer = EqualityComparer<T>.Default;
    }

    public QueueADT(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        m_Array = new T[1];
        Count = 0;
        m_Front = 0;
        m_Comparer = EqualityComparer<T>.Default;
        foreach (T item in collection) Enqueue(item);
    }

    public void Enqueue(T item)
    {
        if (m_Array.Length == 0) m_Array = new T[1];
        else if (m_Front + Count == m_Array.Length) Resize(m_Array.Length * 2);
        m_Array[m_Front + Count++] = item;
    }

    public T Dequeue()
    {
        if (IsEmpty) throw new EmptyQueueException("The queue has no items to access");
        T item = m_Array[m_Front];
        m_Array[m_Front++] = default;
        --Count;
        if (Count < m_Array.Length / 4) Resize(m_Array.Length / 2);
        return item;
    }

    public void Clear()
    {
        m_Array = Array.Empty<T>();
        Count = 0;
        m_Front = 0;
    }

    public bool Contains(T item)
    {
        for (int i = m_Front; i < m_Front + Count; ++i)
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
        if (array.Length - index < Count) throw new ArgumentException("not enough space for the Queue in array");
        for (int i = m_Front; i < m_Front + Count; ++i)
        {
            array[index++] = m_Array[i];
        }
    }

    void Resize(int newCapacity)
    {
        if (m_Front > 0)
        {
            Array.Copy(m_Array, m_Front, m_Array, 0, Count);
            m_Front = 0;
            if (newCapacity > m_Array.Length) return; // After we moved everything to the start of the array there is no longer a need to grow
        }
        if (newCapacity > 0) Array.Resize(ref m_Array, newCapacity);
        else m_Array = Array.Empty<T>();
    }

    T[] m_Array;
    IEqualityComparer<T> m_Comparer;
    int m_Front;

    public struct Enumerator : IEnumerator<T>, IEnumerator
    {
        public T Current => m_CurrentValue;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (m_Index == m_Queue.m_Front + m_Queue.Count) return false;
            m_CurrentValue = m_Queue.m_Array[m_Index++];
            return true;
        }

        public void Reset()
        {
            m_CurrentValue = default;
            m_Index = m_Queue.m_Front;
        }

        internal Enumerator(QueueADT<T> queue)
        {
            m_Queue = queue;
            m_CurrentValue = default;
            m_Index = m_Queue.m_Front;
        }

        QueueADT<T> m_Queue;
        T m_CurrentValue;
        int m_Index;
    }
}

public class EmptyQueueException : Exception
{
    public EmptyQueueException() { }

    public EmptyQueueException(string message) : base(message) { }

    public EmptyQueueException(string message, Exception inner) : base(message, inner) { }
}
