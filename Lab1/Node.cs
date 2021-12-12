using Lab1.Utils;

namespace Lab1;

public class Node<T>{
    private static uint _randomSeed;
    public MarkedReference<T> NodeValue{ get; }

    public int NodeKey{ get; }

    public MarkedReference<Node<T>>[] Next{ get; }

    public int TopLevel{ get; }

    static Node(){
        _randomSeed = (uint) (DateTime.Now.Millisecond) | 0x0100;
    }

    public Node(int key){
        NodeValue = new MarkedReference<T>(default(T), false);
        NodeKey = key;
        Next = new MarkedReference<Node<T>>[Config.MaxLevel + 1];
        for (var i = 0; i < Next.Length; ++i){
            Next[i] = new MarkedReference<Node<T>>(null, false);
        }

        TopLevel = Config.MaxLevel;
    }

    public Node(T value, int key){
        NodeValue = new MarkedReference<T>(value, false);
        NodeKey = key;
        var height = RandomLevel();
        Next = new MarkedReference<Node<T>>[height + 1];
        for (var i = 0; i < Next.Length; ++i){
            Next[i] = new MarkedReference<Node<T>>(null, false);
        }

        TopLevel = height;
    }

    private static int RandomLevel(){
        var x = _randomSeed;
        x ^= x << 13;
        x ^= x >> 17;
        _randomSeed = x ^= x << 5;
        if ((x & 0x80000001) != 0){
            return 0;
        }

        var level = 1;
        while (((x >>= 1) & 1) != 0){
            level++;
        }

        return Math.Min(level, Config.MaxLevel);
    }
}