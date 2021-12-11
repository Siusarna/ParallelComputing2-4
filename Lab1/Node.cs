using Lab1.Utils;

namespace Lab1;

public class Node<T>{
    private MarkedReference<T> NodeValue{ get; }
    public MarkedReference<Node<T>>[] Next{ get; }
    public int NodeKey{ get; }
    public int TopLevel{ get; }

    public Node(int key){
        NodeValue = new MarkedReference<T>(default(T), false);
        NodeKey = key;
        Next = new MarkedReference<Node<T>>[Config.MaxLevel + 1];
        for (var i = 0; i < Next.Length; ++i){
            Next[i] = new MarkedReference<Node<T>>(null!, false);
        }

        TopLevel = Config.MaxLevel;
    }
}