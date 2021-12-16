using Lab1.Utils;

namespace Lab1;

public class HarrisNode{
    public int Value{ get; }

    public MarkedReference<HarrisNode> Next{ get; set; }

    public HarrisNode(int value){
        Value = value;
        Next = new MarkedReference<HarrisNode>(default, false);
    }
}