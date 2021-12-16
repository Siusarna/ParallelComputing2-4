using NUnit.Framework;

namespace Lab1.Tests;

[TestFixture]
public class Performance{
    [Test]
    public void ConcurrentReadWritePerformance(){
        const int count = 1;
        var times = new int[count];
        for (var i = 0; i < count; i++){
            var list = new HarrisList();
            var t = new CollectionReadWritePerformanceLinkedList(list, 10, 3, 5);
            times[i] = t.Run().Milliseconds;
            PrintLinkedList(list);
            Parallel.ForEach(t.SavedValue, (node) => { list.Remove(node); });
            PrintLinkedList(list);
        }

        Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", times.Average(), times.Min(), times.Max());
        Console.WriteLine(string.Join(" ", times));
    }

    private static void PrintLinkedList(HarrisList target){
        Console.WriteLine("----------------------------");
        Console.WriteLine("Start printing...");
        var i = 0;
        var elem = target.Head.Next.Value;
        while (elem is not null && elem != target.Tail){
            Console.Write("{0:00}|", i);
            Console.WriteLine(elem.Value);
            elem = elem.Next.Value;
            i++;
        }

        Console.WriteLine("----------------------------");
    }
}