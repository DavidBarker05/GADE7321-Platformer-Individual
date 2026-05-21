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

	public LinkedListADT(bool isCircular = true) { IsCircular = isCircular; }

	public LinkedListADT(IEnumerable<T> collection, bool isCircular = true)
	{
		if (collection == null) throw new System.ArgumentNullException("collection");
		foreach (T item in collection) AddBack(item);
		IsCircular = isCircular;
	}

	public void AddFront(LinkedListADTNode<T> node)
	{
		if (node == null) throw new System.ArgumentNullException("node is null");
		if (node.List != null) throw new System.ArgumentException("node already belongs to a list");
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
		if (node == null) throw new System.ArgumentNullException("node is null");
		if (node.List != null) throw new System.ArgumentException("node already belongs to a list");
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
		if (node == null) throw new System.ArgumentNullException("node is null");
		if (newNode == null) throw new System.ArgumentNullException("newNode is null");
		if (node.List != this) throw new System.ArgumentException("node doesn't belong to this list");
		if (newNode.List != null) throw new System.ArgumentException("newNode already belongs to a list");
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
		if (node == null) throw new System.ArgumentNullException("node is null");
		if (newNode == null) throw new System.ArgumentNullException("newNode is null");
		if (node.List != this) throw new System.ArgumentException("node doesn't belong to this list");
		if (newNode.List != null) throw new System.ArgumentException("newNode already belongs to a list");
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
			if (current._value.Equals(value)) return current;
			current = current._next == Front ? null : current._next;
		}
		return null;
	}

	public LinkedListADTNode<T> FindLast(T value)
	{
		LinkedListADTNode<T> current = Back;
		while (current != null)
		{
			if (current._value.Equals(value)) return current;
			current = current._previous == Back ? null : current._previous;
		}
		return null;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null) throw new System.ArgumentNullException("array");
		if (arrayIndex < 0 || arrayIndex > array.Length) throw new System.IndexOutOfRangeException("arrayIndex out of range");
		if (array.Length - arrayIndex < Count) throw new System.ArgumentException("not enough space for the LinkedListADT in array");
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
		if (node.List != this) throw new System.ArgumentException("node doesn't belong to this list");
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
			LinkedListADTNode<T> next = current._next == Front ? null : current._next;
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

	public IEnumerator<T> GetEnumerator() => new Enumerator(this);

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		public T Current => m_CurrentValue;

		object IEnumerator.Current => Current;

		public void Dispose() { }

		public bool MoveNext()
		{
			if (m_CurrentNode == null || m_Index == m_List.Count) return false;
			m_CurrentValue = m_CurrentNode.Value;
			m_CurrentNode = m_CurrentNode.Next;
			++m_Index;
			return true;
		}

		public void Reset()
		{
			m_CurrentNode = m_List.Front;
			m_CurrentValue = default;
			m_Index = 0;
		}

		internal Enumerator(LinkedListADT<T> list)
		{
			m_List = list;
			m_CurrentNode = list.Front;
			m_CurrentValue = default;
			m_Index = 0;
		}

		LinkedListADT<T> m_List;
		LinkedListADTNode<T> m_CurrentNode;
		T m_CurrentValue;
		int m_Index;
	}
}

public sealed class LinkedListADTNode<T>
{
	public T Value { get => _value; set => _value = value; }
	public ref T ValueRef => ref _value;

	public LinkedListADTNode<T> Previous => _previous == null || List == null || List.Front == null || List.Back == null ? null : _previous;
	public LinkedListADTNode<T> Next => _next == null || List == null || List.Front == null || List.Back == null ? null : _next;

	public LinkedListADT<T> List { get; internal set; }

	public LinkedListADTNode(T value) { _value = value; }

	internal T _value;
	internal LinkedListADTNode<T> _previous;
	internal LinkedListADTNode<T> _next;

	internal void FreeMemory()
	{
		List = null;
		_previous = null;
		_next = null;
	}
}
