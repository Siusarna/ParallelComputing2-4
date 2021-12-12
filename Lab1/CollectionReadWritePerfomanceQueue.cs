using System.Diagnostics;

namespace Lab1;

internal sealed class CollectionReadWritePerformanceQueue{
    private const int MAX_VALUE = 10000;
    private readonly LockFreeQueue<int> _target;
    private readonly Thread[] _threads;
    private readonly int _iterations;
    private readonly Random rand = new Random();
    public SynchronizedCollection<int> SavedValue{ get; } = new SynchronizedCollection<int>();

    public CollectionReadWritePerformanceQueue(LockFreeQueue<int> target, int readersCount, int writersCount, int iterations){
        _target = target;
        _iterations = iterations;
        var count = writersCount + readersCount;
        _threads = new Thread[writersCount];

        for (var i = 0; i < writersCount; i++){
            _threads[i] = new Thread(Writer);
        }
    }

    public TimeSpan Run(){
        var watcher = new Stopwatch();
        watcher.Start();
        foreach (Thread t in _threads){
            t.Start();
        }

        foreach (Thread t in _threads){
            t.Join();
        }

        watcher.Stop();
        return watcher.Elapsed;
    }

    private void Writer(){
        try{
            for (var i = 0; i < _iterations; i++){
                var random = rand.Next(MAX_VALUE);
                SavedValue.Add(random);
                _target.Insert(random);
            }
        }
        catch (Exception ex){
            Console.WriteLine(ex);
        }
    }
    private void Delete(){
        try{
            for (var i = 0; i < _iterations; i++){
                _target.Remove(out var result);
            }
        }
        catch (Exception ex){
            Console.WriteLine(ex);
        }
    }
}