namespace Lab1;

public class NodeQueue<T>
{
    public NodeQueue<T> Next;
    public T Value { get; set; }

    public NodeQueue(T value, NodeQueue<T> next)
    {
        Value = value;
        Next = next;
    }
}