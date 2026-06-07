using System;
using System.Collections;
using System.Collections.Generic;

public class LinkedListADT<T> : ICollection<T>, IReadOnlyCollection<T>
{
	public LinkedListADTNode<T> Front { get; private set; } = null;
	public LinkedListADTNode<T> Back { get; private set; } = null;
	public bool IsCircular { get; private set; }
	public int Count { get; private set; } = 0;
	public bool IsEmpty => Count <= 0;

	public bool IsReadOnly => false;

	public LinkedListADT(bool isCircular = true)
	{
		IsCircular = isCircular;
		m_Comparer = EqualityComparer<T>.Default;
	}

	public LinkedListADT(IEnumerable<T> collection, bool isCircular = true)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		IsCircular = isCircular;
		m_Comparer = EqualityComparer<T>.Default;
		foreach (T item in collection) AddBack(item);
	}

	public void AddFront(LinkedListADTNode<T> node)
	{
		if (node == null) throw new ArgumentNullException("node is null");
		if (node.List != null) throw new ArgumentException("node already belongs to a list");
		if (Front == null) CreateList(node);
		else InternalAddBefore(Front, node);
	}

	public LinkedListADTNode<T> AddFront(T value)
	{
		LinkedListADTNode<T> node = new LinkedListADTNode<T>(value);
		AddFront(node);
		return node;
	}

	public void AddBack(LinkedListADTNode<T> node)
	{
		if (node == null) throw new ArgumentNullException("node is null");
		if (node.List != null) throw new ArgumentException("node already belongs to a list");
		if (Back == null) CreateList(node);
		else InternalAddAfter(Back, node);
	}

	public LinkedListADTNode<T> AddBack(T value)
	{
		LinkedListADTNode<T> node = new LinkedListADTNode<T>(value);
		AddBack(node);
		return node;
	}

	public void AddBefore(LinkedListADTNode<T> node, LinkedListADTNode<T> newNode)
	{
		if (node == null) throw new ArgumentNullException("node is null");
		if (newNode == null) throw new ArgumentNullException("newNode is null");
		if (node.List != this) throw new ArgumentException("node doesn't belong to this list");
		if (newNode.List != null) throw new ArgumentException("newNode already belongs to a list");
		InternalAddBefore(node, newNode);
	}

	public LinkedListADTNode<T> AddBefore(LinkedListADTNode<T> node, T value)
	{
		LinkedListADTNode<T> newNode = new LinkedListADTNode<T>(value);
		AddBefore(node, newNode);
		return newNode;
	}

	public void AddAfter(LinkedListADTNode<T> node, LinkedListADTNode<T> newNode)
	{
		if (node == null) throw new ArgumentNullException("node is null");
		if (newNode == null) throw new ArgumentNullException("newNode is null");
		if (node.List != this) throw new ArgumentException("node doesn't belong to this list");
		if (newNode.List != null) throw new ArgumentException("newNode already belongs to a list");
		InternalAddAfter(node, newNode);
	}

	public LinkedListADTNode<T> AddAfter(LinkedListADTNode<T> node, T value)
	{
		LinkedListADTNode<T> newNode = new LinkedListADTNode<T>(value);
		AddAfter(node, newNode);
		return newNode;
	}

	public void Add(T item) => AddBack(item);

	public LinkedListADTNode<T> FindFirst(T value)
	{
		LinkedListADTNode<T> current = Front;
		while (current != null)
		{
			if (m_Comparer.Equals(current._value, value)) return current;
			current = current != Back ? current._next : null;
		}
		return null;
	}

	public LinkedListADTNode<T> FindFirst(Predicate<LinkedListADTNode<T>> predicate)
	{
		LinkedListADTNode<T> current = Front;
		while (current != null)
		{
			if (predicate(current)) return current;
			current = current != Back ? current._next : null;
		}
		return null;
	}

	public LinkedListADTNode<T> FindFirst(Predicate<T> predicate)
	{
		LinkedListADTNode<T> current = Front;
		while (current != null)
		{
			if (predicate(current._value)) return current;
			current = current != Back ? current._next : null;
		}
		return null;
	}

	public LinkedListADTNode<T> FindLast(T value)
	{
		LinkedListADTNode<T> current = Back;
		while (current != null)
		{
			if (m_Comparer.Equals(current._value, value)) return current;
			current = current != Front ? current._previous : null;
		}
		return null;
	}

	public LinkedListADTNode<T> FindLast(Predicate<LinkedListADTNode<T>> predicate)
	{
		LinkedListADTNode<T> current = Back;
		while (current != null)
		{
			if (predicate(current)) return current;
			current = current != Front ? current._previous : null;
		}
		return null;
	}

	public LinkedListADTNode<T> FindLast(Predicate<T> predicate)
	{
		LinkedListADTNode<T> current = Back;
		while (current != null)
		{
			if (predicate(current._value)) return current;
			current = current != Front ? current._previous : null;
		}
		return null;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException("array");
		if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException("arrayIndex out of range");
		if (array.Length - arrayIndex < Count) throw new ArgumentException("not enough space for the LinkedListADT in array");
		LinkedListADTNode<T> current = Front;
		while (current != null)
		{
			array[arrayIndex++] = current.Value;
			current = current != Back ? current.Next : null;
		}
	}

	public void Remove(LinkedListADTNode<T> node)
	{
		if (node == null) return;
		if (node.List != this) throw new ArgumentException("node doesn't belong to this list");
		InternalRemove(node);
	}

	public void RemoveFront()
	{
		if (Front == null) return;
		InternalRemove(Front);
	}

	public void RemoveBack()
	{
		if (Back == null) return;
		InternalRemove(Back);
	}

	public void RemoveFirst(T value)
	{
		LinkedListADTNode<T> node = FindFirst(value);
		if (node != null) InternalRemove(node);
	}

	public void RemoveLast(T value)
	{
		LinkedListADTNode<T> node = FindLast(value);
		if (node != null) InternalRemove(node);
	}

	public bool Remove(T item)
	{
		LinkedListADTNode<T> node = FindFirst(item);
		if (node != null)
		{
			Remove(node);
			return true;
		}
		return false;
	}

	public bool Contains(LinkedListADTNode<T> node) => node.List == this;

	public bool Contains(T value) => FindFirst(value) != null;

	public void Clear()
	{
		if (Count == 0) return;
		LinkedListADTNode<T> current = Front;
		while (current != null)
		{
			LinkedListADTNode<T> next = current != Back ? current._next : null;
			current.FreeMemory();
			current = next;
		}
		Front = null;
		Back = null;
		Count = 0;
	}

	private void CreateList(LinkedListADTNode<T> node)
	{
		node.List = this;
		node._next = IsCircular ? node : null;
		node._previous = IsCircular ? node : null;
		Front = node;
		Back = node;
		Count = 1;
	}

	private void InternalAddBefore(LinkedListADTNode<T> node, LinkedListADTNode<T> newNode)
	{
		newNode.List = this;
		newNode._next = node;
		newNode._previous = node._previous;
		if (node._previous != null) node._previous._next = newNode;
		node._previous = newNode;
		if (node == Front) Front = newNode;
		++Count;
	}

	private void InternalAddAfter(LinkedListADTNode<T> node, LinkedListADTNode<T> newNode)
	{
		newNode.List = this;
		newNode._previous = node;
		newNode._next = node._next;
		if (node._next != null) node._next._previous = newNode;
		node._next = newNode;
		if (node == Back) Back = newNode;
		++Count;
	}

	private void InternalRemove(LinkedListADTNode<T> node)
	{
		if (Count == 0) return;
		if (Count == 1)
		{
			Front = null;
			Back = null;
		}
		else
		{
			if (node == Front) Front = Front._next;
			if (node == Back) Back = Back._previous;
			if (node._previous != null) node._previous._next = node._next;
			if (node._next != null) node._next._previous = node._previous;
		}
		node.FreeMemory();
		--Count;
	}

	public Enumerator GetEnumerator() => new Enumerator(this);

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	IEqualityComparer<T> m_Comparer;

	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		public T Current => m_CurrentValue;

		object IEnumerator.Current => Current;

		public void Dispose() { }

		public bool MoveNext()
		{
			if (!m_CurrentNode) return false;
			m_CurrentValue = m_CurrentNode.Value;
			m_CurrentNode = m_CurrentNode != m_List.Back ? m_CurrentNode.Next : null;
			return true;
		}

		public void Reset()
		{
			m_CurrentNode = m_List.Front;
			m_CurrentValue = default;
		}

		internal Enumerator(LinkedListADT<T> list)
		{
			m_List = list;
			m_CurrentNode = list.Front;
			m_CurrentValue = default;
		}

		LinkedListADT<T> m_List;
		LinkedListADTNode<T> m_CurrentNode;
		T m_CurrentValue;
	}
}
