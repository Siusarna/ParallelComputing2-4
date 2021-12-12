namespace Lab1;

public class LockFreeQueue<T>{
    private NodeQueue<T> _head;
    private NodeQueue<T> _tail;

    public NodeQueue<T> Head => _head;
    public NodeQueue<T> Tail => _tail;

    public LockFreeQueue(){
        _head = new NodeQueue<T>(default, null);
        _tail = _head;
    }

    private bool AddSchema(ref NodeQueue<T> tail, NodeQueue<T> node){
        var tailNext = tail.Next;
        if (tailNext is null){
            if (Interlocked.CompareExchange(ref tail.Next, node, tailNext) != tailNext){
                return false;
            }

            Interlocked.CompareExchange(ref _tail, node, tail);
            return true;
        }

        Interlocked.CompareExchange(ref _tail, tailNext, tail);
        return false;
    }

    public void Insert(T value){
        var node = new NodeQueue<T>(value, null);

        while (true){
            var tail = _tail;
            if (tail == null || tail != _tail){
                continue;
            }

            var isReturn = AddSchema(ref tail, node);
            if (isReturn){
                return;
            }
        }
    }

    private bool? RemoveSchema(ref NodeQueue<T> head, ref NodeQueue<T> tail, ref T result){
        var next = head.Next;
        if (next is null){
            result = default(T);
            return false;
        }

        if (head == tail){
            Interlocked.CompareExchange(ref _tail, next, tail);
            return null;
        }

        result = next.Value;
        if (Interlocked.CompareExchange(ref _head, next, head) == head){
            return true;
        }

        return null;
    }

    public bool Remove(out T result){
        result = default(T);
        while (true){
            var head = _head;
            var tail = _tail;
            if (head == null || head != _head){
                continue;
            }

            var isRemoved = RemoveSchema(ref head, ref tail, ref result);
            if (isRemoved is null){
                continue;
            }
            return (bool)isRemoved;
        }
    }
}