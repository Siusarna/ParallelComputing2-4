namespace Lab1.Atomics;

public class AtomicBool{
    private long _currentValue;

    public AtomicBool(bool initialValue){
        _currentValue = Convert.ToInt32(initialValue);
    }

    public bool GetValue(){
        return Convert.ToBoolean(Interlocked.Read(ref _currentValue));
    }

    public bool SetValue(bool newValue){
        return Convert.ToBoolean(Interlocked.Exchange(ref _currentValue, Convert.ToInt32(newValue)));
    }
}