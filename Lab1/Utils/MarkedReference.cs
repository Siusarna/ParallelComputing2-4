using Lab1.Atomics;

namespace Lab1.Utils;

public class MarkedReference<T>{
    private readonly AtomicReference<ReferenceBase<T>> _reference;

    public MarkedReference(T value, bool marked){
        _reference = new AtomicReference<ReferenceBase<T>>(new ReferenceBase<T>(value, marked));
    }

    public T Get(ref bool marked){
        marked = Marked.GetValue();
        return Value;
    }

    public T Value => _reference.Value.Value;

    public AtomicBool Marked => _reference.Value.Marked;

    public bool CompareAndExchange(T newValue, bool newMarked, T oldValue, bool oldMarked){
        var oldReference = _reference.Value;

        if (!ReferenceEquals(oldReference.Value, oldValue)){
            return false;
        }

        if (oldReference.Marked.GetValue() != oldMarked){
            return false;
        }

        return _reference.CompareAndExchange(new ReferenceBase<T>(newValue, newMarked), oldReference);
    }
}