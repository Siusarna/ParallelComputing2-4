using NUnit.Framework;

namespace Lab1;

[TestFixture]
public class Performance{
    [Test]
    public void ConcurrentReadWritePerformance(){
        const int count = 1;
        var times = new int[count];
        for (var i = 0; i < count; i++){
            var list = new SkipListLockFree<int>();
            var t = new CollectionReadWritePerformance(list, 10, 3, 5);
            times[i] = t.Run().Milliseconds;
            PrintSkipListForm(list);
            foreach (var node in t.SavedValue){
                Console.WriteLine(node.NodeValue.Value);
            }
            Parallel.ForEach(t.SavedValue, (node) =>
            {
                list.Delete(node);
            });
            PrintSkipListForm(list);
        }
        
        Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", times.Average(), times.Min(), times.Max());
        Console.WriteLine(string.Join(" ", times));
    }
    private static void PrintSkipListForm<T>(SkipListLockFree<T> target) where T : IComparable<T>{
        for (int i = Config.MaxLevel; i >= 0; i--){
            Console.Write("{0:00}|", i);
            bool marked = false;
            var node = target.Head.Next[i].Get(ref marked);
            while (node != target.Tail){
                Console.Write(node.TopLevel >= i ? $"{node.NodeValue.Value} " : " ");
                node = node.Next[i].Get(ref marked);
            }
    
            Console.WriteLine();
        }
    
        Console.WriteLine("----------------------------");
    }
}