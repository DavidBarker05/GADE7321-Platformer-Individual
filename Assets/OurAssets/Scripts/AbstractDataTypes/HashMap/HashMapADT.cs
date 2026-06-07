using System;
using System.Collections;
using System.Collections.Generic;

// This should not count less marks than the GraphADT. The hashing
// and buckets are standard and make doing this way more complex
// than a graph.

// This code ended up being a mix of functionality from c# and c++
// combining some of the feature I prefer from both
public class HashMapADT<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
    public int Count { get; private set; }

    public bool IsReadOnly => false;

    public HashMapADT() : this(0) { } // Default to 0 capacity

    public HashMapADT(int capacity)
    {
        if (capacity < 0) throw new ArgumentException("capacity needs to be greater than or equal to 0");
        if (capacity > 0) Initialise(capacity);
        else m_Buckets = Array.Empty<LinkedListADT<KeyValuePair<TKey, TValue>>>();
        Count = 0;
        m_KeyComparer = EqualityComparer<TKey>.Default;
        m_ValueComparer = EqualityComparer<TValue>.Default;
    }

    public HashMapADT(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        Initialise(1);
        Count = 0;
        m_KeyComparer = EqualityComparer<TKey>.Default;
        m_ValueComparer = EqualityComparer<TValue>.Default;
        foreach (var item in collection) Add(item);
    }

    public TValue this[TKey key]
    {
        get
        {
            var entry = GetEntry(key);
            if (entry) return entry.Value.Value;
            return default;
        }
        set => Insert(key, value); // Update key's value or insert if it doesn't exist similar to c++
    }

    public bool ContainsKey(TKey key) => GetEntry(key);

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
        Insert(item.Key, item.Value);
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Insert(key, value);
    }

    public void Clear()
    {
        m_Buckets = Array.Empty<LinkedListADT<KeyValuePair<TKey, TValue>>>();
        m_Capacity = 0;
        Count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
        var entry = GetEntry(item.Key);
        return entry && m_ValueComparer.Equals(entry.Value.Value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException($"{nameof(arrayIndex)} out of range");
        if (array.Length - arrayIndex < Count) throw new ArgumentException($"not enough space for the HashMapADT in {nameof(array)}");
        foreach (var bucket in m_Buckets)
        {
            if (bucket.IsEmpty) continue;
            bucket.CopyTo(array, arrayIndex);
            arrayIndex += bucket.Count;
        }
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
        var entry = GetEntry(item.Key);
        if (!entry || !m_ValueComparer.Equals(entry.Value.Value, item.Value)) return false;
        int bucketIndex = BucketIndex(item.Key);
        m_Buckets[bucketIndex].Remove(entry);
        if (--Count < m_Capacity / 4) Resize(m_Capacity / 2);
        return true;
    }

    public void Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        var entry = GetEntry(key);
        if (!entry) return;
        int bucketIndex = BucketIndex(key);
        m_Buckets[bucketIndex].Remove(entry);
        if (--Count < m_Capacity / 4) Resize(m_Capacity / 2);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

    int BucketIndex(TKey key)
    {
        int hashCode = m_KeyComparer.GetHashCode(key) & int.MaxValue; // Keep positive
        return hashCode % m_Buckets.Length;
    }

    LinkedListADTNode<KeyValuePair<TKey, TValue>> GetEntry(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (m_Buckets.Length == 0) return null;
        int bucketIndex = BucketIndex(key);
        if (m_Buckets[bucketIndex].IsEmpty) return null;
        return m_Buckets[bucketIndex].FindFirst(kvp => m_KeyComparer.Equals(kvp.Key, key));
    }

    void Initialise(int capacity)
    {
        m_Capacity = capacity;
        m_Buckets = new LinkedListADT<KeyValuePair<TKey, TValue>>[Math.Max(1, capacity / 2)];
        PopulateBuckets();
    }

    void Insert(TKey key, TValue value)
    {
        if (m_Capacity == 0) Initialise(1);
        int bucketIndex = BucketIndex(key);
        if (m_Buckets[bucketIndex].Count > 0)
        {
            var entry = m_Buckets[bucketIndex].FindFirst(kvp => m_KeyComparer.Equals(kvp.Key, key));
            if (entry)
            {
                entry.Value = new KeyValuePair<TKey, TValue>(key, value);
                return;
            }
        }
        if (Count == m_Capacity)
        {
            Resize(m_Capacity * 2);
            bucketIndex = BucketIndex(key);
        }
        m_Buckets[bucketIndex].AddBack(new KeyValuePair<TKey, TValue>(key, value));
        ++Count;
    }

    void PopulateBuckets()
    {
        for (int i = 0; i < m_Buckets.Length; ++i)
        {
            m_Buckets[i] = new LinkedListADT<KeyValuePair<TKey, TValue>>(false);
        }
    }

    void Resize(int newCapacity)
    {
        m_Capacity = newCapacity;
        Count = 0;
        if (newCapacity == 0)
        {
            m_Buckets = Array.Empty<LinkedListADT<KeyValuePair<TKey, TValue>>>();
            return;
        }
        var entries = new KeyValuePair<TKey, TValue>[Count];
        CopyTo(entries, 0);
        if (newCapacity == 1) m_Buckets = new LinkedListADT<KeyValuePair<TKey, TValue>>[1];
        else m_Buckets = new LinkedListADT<KeyValuePair<TKey, TValue>>[newCapacity / 2];
        PopulateBuckets();
        foreach (var entry in entries) Insert(entry.Key, entry.Value);
    }

    LinkedListADT<KeyValuePair<TKey, TValue>>[] m_Buckets;
    int m_Capacity;
    IEqualityComparer<TKey> m_KeyComparer;
    IEqualityComparer<TValue> m_ValueComparer;

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        public KeyValuePair<TKey, TValue> Current => m_Current?.Value ?? default;

        object IEnumerator.Current => Current;

        internal Enumerator(HashMapADT<TKey, TValue> hashMap)
        {
            m_HashMap = hashMap;
            m_BucketIndex = 0;
            m_Current = null;
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            if (!m_Current || m_Current == m_HashMap.m_Buckets[m_BucketIndex].Back)
            {
                while (m_BucketIndex < m_HashMap.m_Buckets.Length)
                {
                    var bucket = m_HashMap.m_Buckets[m_BucketIndex++];
                    if (bucket.IsEmpty) continue;
                    m_Current = bucket.Front;
                    return true;
                }
                return false;
            }
            m_Current = m_Current.Next;
            return true;
        }

        public void Reset()
        {
            m_BucketIndex = 0;
            m_Current = null;
        }

        HashMapADT<TKey, TValue> m_HashMap;
        int m_BucketIndex;
        LinkedListADTNode<KeyValuePair<TKey, TValue>> m_Current;
    }
}
