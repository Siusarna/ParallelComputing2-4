using NUnit.Framework;

namespace Lab1;

[TestFixture]
public class PerformanceQueue{
    [Test]
    public void ConcurrentWritePerformance(){
        const int count = 1;
        var times = new int[count];
        for (var i = 0; i < count; i++){
            var queue = new LockFreeQueue<int>();
            var t = new CollectionWritePerformanceQueue(queue, 2, 5);
            times[i] = t.Run().Milliseconds;
            PrintQueueForm(queue);
            Parallel.ForEach(t.SavedValue, (el) =>
            {
                queue.Remove(out int result);
                Console.WriteLine($"{result} was removed");
            });
            PrintQueueForm(queue);
        }
        
        Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", times.Average(), times.Min(), times.Max());
        Console.WriteLine(string.Join(" ", times));
    }
    private static void PrintQueueForm<T>(LockFreeQueue<T> target) where T : IComparable<T>{
        Console.WriteLine("----------------------------");
        Console.WriteLine("Start printing...");
        var i = 0;
        var elem = target.Head.Next;
        while (elem is not null){
            Console.Write("{0:00}|", i);
            Console.WriteLine(elem.Value);
            elem = elem.Next;
            i++;
        }

        Console.WriteLine("----------------------------");
    }
}