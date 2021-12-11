namespace Lab1.Atomics;

public class AtomicReference<T> where T : class{
    private T _value;

    public AtomicReference(T value){
        _value = value;
    }

    public T Value{
        get{
            object obj = _value;
            return (T) Thread.VolatileRead(ref obj);
        }
    }

    public bool CompareAndExchange(T newValue, T oldValue){
        return ReferenceEquals(Interlocked.CompareExchange(ref _value, newValue, oldValue), oldValue);
    }
}