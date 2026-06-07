public sealed class LinkedListADTNode<T>
{
    public T Value { get => _value; set => _value = value; }
    public ref T ValueRef => ref _value;

    public static implicit operator bool(LinkedListADTNode<T> node) => node != null;

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