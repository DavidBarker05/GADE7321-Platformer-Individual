using System.Collections.Generic;

public class GraphADTNode<T>
{
    public T Value { get => _value; set => _value = value; }
    public ref T ValueRef => ref _value;

    public static implicit operator bool(GraphADTNode<T> node) => node != null;

    public int NumConnections => _connections.Count;
    public bool HaConnections => _connections.Count > 0;

    public GraphADTNode<T>[] Connections => _connections.ToArray();

    public GraphADT<T> Graph { get; internal set; }

    public GraphADTNode()
    {
        _value = default;
        _connections = new List<GraphADTNode<T>>();
        Graph = null;
    }

    public GraphADTNode(T value)
    {
        _value = value;
        _connections = new List<GraphADTNode<T>>();
        Graph = null;
    }

    internal GraphADTNode(T value, GraphADT<T> graph)
    {
        _value = value;
        _connections = new List<GraphADTNode<T>>();
        Graph = graph;
    }

    internal T _value;
    internal List<GraphADTNode<T>> _connections;
}
