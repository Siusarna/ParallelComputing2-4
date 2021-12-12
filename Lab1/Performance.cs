using NUnit.Framework;

namespace Lab1;

[TestFixture]
public class Performance{
    [Test]
    public void ConcurrentReadWritePerformance(){
        const int count = 10;
        var times = new int[count];
        for (var i = 0; i < count; i++){
            var c = new SkipListLockFree<int>();
            var t = new CollectionReadWritePerformance(c, 10, 1, 10000);
            times[i] = t.Run().Milliseconds;
            Console.WriteLine("bla");
        }

        Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", times.Average(), times.Min(), times.Max());
        Console.WriteLine(string.Join(" ", times));
    }

    [Test]
    public void Population(){
        var target = new SkipListLockFree<int>();
        for (int i = 0; i < 10000; i++){
            var node = new Node<int>(i, i);
            target.Insert(node);
        }

        // PrintSkipListForm(target);
    }

    // private static void PrintSkipListForm<T>(SkipListLockFree<T> target) where T : IComparable<T>{
    //     for (int i = target._height; i >= 0; i--){
    //         Console.Write("{0:00}|", i);
    //         bool marked = false;
    //         var node = target.Head.Next[i].Get(ref marked);
    //         while (node != target.Tail){
    //             Console.Write(node.Height >= i ? "*" : " ");
    //             node = node.Next[i].Get(ref marked);
    //         }
    //
    //         Console.WriteLine();
    //     }
    //
    //     Console.WriteLine("----------------------------");
    // }
}