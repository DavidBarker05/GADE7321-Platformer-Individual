using System.Collections.Generic;

public class GraphADT<T>
{
    public bool IsEmpty => m_Nodes.Count == 0;
    public int Count => m_Nodes.Count;

    public GraphADTNode<T>[] Nodes => m_Nodes.ToArray();

    public GraphADT()
    {
        m_Nodes = new List<GraphADTNode<T>>();
        m_Comparer = EqualityComparer<T>.Default;
    }

    public GraphADTNode<T> CreateNode(T value)
    {
        GraphADTNode<T> node = new GraphADTNode<T>(value, this);
        m_Nodes.Add(node);
        return node;
    }

    public void AddNode(GraphADTNode<T> node)
    {
        if (node.Graph != null) throw new System.ArgumentException("Node already belongs to another graph");
        node.Graph = this;
        m_Nodes.Add(node);
    }

    public void RemoveNode(GraphADTNode<T> node)
    {
        if (node.Graph != this) throw new System.ArgumentException("Node doesn't belong to this graph");
        foreach (GraphADTNode<T> connection in node._connections)
        {
            connection._connections.Remove(node);
        }
        node._value = default;
        node._connections.Clear();
        node.Graph = null;
        m_Nodes.Remove(node);
    }

    public void CreateConnection(GraphADTNode<T> nodeA, GraphADTNode<T> nodeB)
    {
        nodeA.Graph ??= this;
        nodeB.Graph ??= this;
        if (nodeA.Graph != this) throw new System.ArgumentException("nodeA doesn't belong to this graph");
        if (nodeB.Graph != this) throw new System.ArgumentException("nodeB doesn't belong to this graph");
        if (nodeA._connections.Contains(nodeB)) return; // Don't want to add duplicate connections
        nodeA._connections.Add(nodeB);
        nodeB._connections.Add(nodeA);
    }

    public void RemoveConnection(GraphADTNode<T> nodeA, GraphADTNode<T> nodeB)
    {
        if (nodeA.Graph != this) throw new System.ArgumentException("nodeA doesn't belong to this graph");
        if (nodeB.Graph != this) throw new System.ArgumentException("nodeB doesn't belong to this graph");
        nodeA._connections.Remove(nodeB);
        nodeB._connections.Remove(nodeA);
    }

    public void Clear()
    {
        while (m_Nodes.Count > 0)
        {
            RemoveNode(m_Nodes[0]);
        }
    }

    public bool Contains(T value)
    {
        foreach (GraphADTNode<T> node in m_Nodes)
        {
            if (m_Comparer.Equals(node._value, value)) return true;
        }
        return false;
    }

    /// <summary>
    /// <para>Searches through every node in the graph as a list to find the first instance of the value. Does not</para>
    /// <para>use any search algorithms, it just finds any node with the value. The node might be disconnected</para>
    /// <para>and unreachable</para>
    /// </summary>
    /// <param name="value">The value to find</param>
    /// <returns>The node with the matching value or null if not found</returns>
    public GraphADTNode<T> Find(T value)
    {
        foreach (GraphADTNode<T> node in m_Nodes)
        {
            if (m_Comparer.Equals(node._value, value)) return node;
        }
        return null;
    }

    /// <summary>
    /// Searches through every node in the graph as a list to find the first instance of the value and removes it.
    /// </summary>
    /// <param name="value">The value to remove</param>
    public void Remove(T value)
    {
        foreach (GraphADTNode<T> node in m_Nodes)
        {
            if (m_Comparer.Equals(node._value, value))
            {
                RemoveNode(node);
                break;
            }
        }
    }

    public GraphADTNode<T>[] GetPathBFS(GraphADTNode<T> startNode, GraphADTNode<T> endNode)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        if (endNode.Graph != this) throw new System.ArgumentException("endNode doesn't belong to this graph");
        QueueADT<GraphADTNode<T>> toSearch = new QueueADT<GraphADTNode<T>>();
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        HashMapADT<GraphADTNode<T>, GraphADTNode<T>> previousNodes = new HashMapADT<GraphADTNode<T>, GraphADTNode<T>>(Count);
        toSearch.Enqueue(startNode);
        searched.Add(startNode);
        previousNodes[startNode] = null;
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Dequeue();
            if (current == endNode)
            {
                StackADT<GraphADTNode<T>> path = new StackADT<GraphADTNode<T>>();
                GraphADTNode<T> _current = endNode;
                while (_current != null)
                {
                    path.Push(current);
                    _current = previousNodes[_current];
                }
                return path.ToArray();
            }
            foreach (GraphADTNode<T> connection in current._connections)
            {
                if (searched.Contains(connection)) continue;
                toSearch.Enqueue(connection);
                searched.Add(connection);
                previousNodes[connection] = current;
            }
        }
        return null;
    }

    public GraphADTNode<T>[] GetPathBFS(GraphADTNode<T> startNode, T value)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        QueueADT<GraphADTNode<T>> toSearch = new QueueADT<GraphADTNode<T>>();
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        HashMapADT<GraphADTNode<T>, GraphADTNode<T>> previousNodes = new HashMapADT<GraphADTNode<T>, GraphADTNode<T>>(Count);
        toSearch.Enqueue(startNode);
        searched.Add(startNode);
        previousNodes[startNode] = null;
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Dequeue();
            if (m_Comparer.Equals(current._value, value))
            {
                StackADT<GraphADTNode<T>> path = new StackADT<GraphADTNode<T>>();
                GraphADTNode<T> _current = current;
                while (_current != null)
                {
                    path.Push(current);
                    _current = previousNodes[_current];
                }
                return path.ToArray();
            }
            foreach (GraphADTNode<T> connection in current._connections)
            {
                if (searched.Contains(connection)) continue;
                toSearch.Enqueue(connection);
                searched.Add(connection);
                previousNodes[connection] = current;
            }
        }
        return null;
    }

    public GraphADTNode<T> FindBFS(GraphADTNode<T> startNode, T value)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        QueueADT<GraphADTNode<T>> toSearch = new QueueADT<GraphADTNode<T>>();
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        toSearch.Enqueue(startNode);
        searched.Add(startNode);
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Dequeue();
            if (m_Comparer.Equals(current._value, value)) return current;
            foreach (GraphADTNode<T> connection in current._connections)
            {
                if (searched.Contains(connection)) continue;
                toSearch.Enqueue(connection);
                searched.Add(connection);
            }
        }
        return null;
    }

    public void RemoveBFS(GraphADTNode<T> startNode, T value)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        QueueADT<GraphADTNode<T>> toSearch = new QueueADT<GraphADTNode<T>>();
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        toSearch.Enqueue(startNode);
        searched.Add(startNode);
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Dequeue();
            if (m_Comparer.Equals(current._value, value))
            {
                RemoveNode(current);
                break;
            }
            foreach (GraphADTNode<T> connection in current._connections)
            {
                if (searched.Contains(connection)) continue;
                toSearch.Enqueue(connection);
                searched.Add(connection);
            }
        }
    }

    public GraphADTNode<T>[] GetPathDFS(GraphADTNode<T> startNode, GraphADTNode<T> endNode)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        if (endNode.Graph != this) throw new System.ArgumentException("endNode doesn't belong to this graph");
        StackADT<GraphADTNode<T>> toSearch = new StackADT<GraphADTNode<T>>();
        HashMapADT<GraphADTNode<T>, int> nextConnectionIndex = new HashMapADT<GraphADTNode<T>, int>(Count);
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        List<GraphADTNode<T>> path = new List<GraphADTNode<T>>();
        toSearch.Push(startNode);
        nextConnectionIndex[startNode] = 0;
        searched.Add(startNode);
        path.Add(startNode);
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Top;
            if (current == endNode || nextConnectionIndex[current] == current.NumConnections)
            {
                if (current == endNode) return path.ToArray();
                path.RemoveAt(path.Count - 1); // No pop_back like c++ vector :(
                toSearch.Pop();
            }
            else
            {
                GraphADTNode<T> next = current._connections[nextConnectionIndex[current]];
                ++nextConnectionIndex[current];
                if (searched.Contains(next)) continue;
                toSearch.Push(next);
                nextConnectionIndex[next] = 0;
                searched.Add(next);
                path.Add(next);
            }
        }
        return null;
    }

    public GraphADTNode<T>[] GetPathDFS(GraphADTNode<T> startNode, T value)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        StackADT<GraphADTNode<T>> toSearch = new StackADT<GraphADTNode<T>>();
        HashMapADT<GraphADTNode<T>, int> nextConnectionIndex = new HashMapADT<GraphADTNode<T>, int>(Count);
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        List<GraphADTNode<T>> path = new List<GraphADTNode<T>>();
        toSearch.Push(startNode);
        nextConnectionIndex[startNode] = 0;
        searched.Add(startNode);
        path.Add(startNode);
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Top;
            if (m_Comparer.Equals(current._value, value) || nextConnectionIndex[current] == current.NumConnections)
            {
                if (m_Comparer.Equals(current._value, value)) return path.ToArray();
                path.RemoveAt(path.Count - 1);
                toSearch.Pop();
            }
            else
            {
                GraphADTNode<T> next = current._connections[nextConnectionIndex[current]];
                ++nextConnectionIndex[current];
                if (searched.Contains(next)) continue;
                toSearch.Push(next);
                nextConnectionIndex[next] = 0;
                searched.Add(next);
                path.Add(next);
            }
        }
        return null;
    }

    public GraphADTNode<T> FindDFS(GraphADTNode<T> startNode, T value)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        StackADT<GraphADTNode<T>> toSearch = new StackADT<GraphADTNode<T>>();
        HashMapADT<GraphADTNode<T>, int> nextConnectionIndex = new HashMapADT<GraphADTNode<T>, int>(Count);
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        toSearch.Push(startNode);
        nextConnectionIndex[startNode] = 0;
        searched.Add(startNode);
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Top;
            if (m_Comparer.Equals(current._value, value) || nextConnectionIndex[current] == current.NumConnections)
            {
                if (m_Comparer.Equals(current._value, value)) return current;
                toSearch.Pop();
            }
            else
            {
                GraphADTNode<T> next = current._connections[nextConnectionIndex[current]];
                ++nextConnectionIndex[current];
                if (searched.Contains(next)) continue;
                toSearch.Push(next);
                nextConnectionIndex[next] = 0;
                searched.Add(next);
            }
        }
        return null;
    }

    public void RemoveDFS(GraphADTNode<T> startNode, T value)
    {
        if (startNode.Graph != this) throw new System.ArgumentException("startNode doesn't belong to this graph");
        StackADT<GraphADTNode<T>> toSearch = new StackADT<GraphADTNode<T>>();
        HashMapADT<GraphADTNode<T>, int> nextConnectionIndex = new HashMapADT<GraphADTNode<T>, int>(Count);
        HashSet<GraphADTNode<T>> searched = new HashSet<GraphADTNode<T>>(Count);
        toSearch.Push(startNode);
        nextConnectionIndex[startNode] = 0;
        searched.Add(startNode);
        while (!toSearch.IsEmpty)
        {
            GraphADTNode<T> current = toSearch.Top;
            if (m_Comparer.Equals(current._value, value) || nextConnectionIndex[current] == current.NumConnections)
            {
                if (m_Comparer.Equals(current._value, value))
                {
                    RemoveNode(current);
                    break;
                }
                toSearch.Pop();
            }
            else
            {
                GraphADTNode<T> next = current._connections[nextConnectionIndex[current]];
                ++nextConnectionIndex[current];
                if (searched.Contains(next)) continue;
                toSearch.Push(next);
                nextConnectionIndex[next] = 0;
                searched.Add(next);
            }
        }
    }

    List<GraphADTNode<T>> m_Nodes;
    IEqualityComparer<T> m_Comparer;
}
