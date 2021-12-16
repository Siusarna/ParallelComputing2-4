using Lab1.Utils;

namespace Lab1;

public class HarrisList{
    public HarrisNode Head{ get; }

    public HarrisNode Tail{ get; }

    public HarrisList(){
        Head = new HarrisNode(Int32.MinValue);
        Tail = new HarrisNode(Int32.MaxValue);

        while (!Head.Next.CompareAndExchange(Tail, false, default, false)){ }
    }

    private bool? AddSchema(int value, ref HarrisNode node){
        HarrisNode pred = null;
        var curr = Search(value, ref pred);
        if (curr != Tail && curr.Value == value){
            return false;
        }

        node.Next = new MarkedReference<HarrisNode>(curr, false);
        if (pred.Next.CompareAndExchange(node, false, curr, false)){
            return true;
        }

        return null;
    }

    public bool Add(int value){
        var node = new HarrisNode(value);

        while (true){
            var res = AddSchema(value, ref node);
            if (res is not null){
                return (bool) res;
            }
        }
    }

    private bool? RemoveSchema(int value){
        HarrisNode pred = null!;
        var curr = Search(value, ref pred);

        if (curr.Value != value){
            return false;
        }

        var rightNext = curr.Next.Value;

        var snip = curr.Next.AttemptMark(rightNext, true);
        if (!snip){
            return null;
        }

        pred.Next.CompareAndExchange(curr, false, rightNext, false);
        return true;
    }

    public bool Remove(int value){
        while (true){
            var res = RemoveSchema(value);
            if (res is not null){
                return (bool) res;
            }
        }
    }

    private HarrisNode? SearchSchema(int searchValue, ref HarrisNode leftNode, ref HarrisNode head,
        ref HarrisNode headNext, ref bool marked){
        var succ = headNext.Next.Get(ref marked);
        while (marked){
            var snip = head.Next.CompareAndExchange(succ, false, headNext, false);
            if (!snip){
                throw new IsComparedNotFoundException("Not found");
            }

            headNext = head.Next.Value;
            succ = headNext.Next.Get(ref marked);
        }

        if (headNext.Value < searchValue){
            head = headNext;
            headNext = succ;
            return null;
        }

        leftNode = head;
        return headNext;
    }

    public HarrisNode Search(int searchValue, ref HarrisNode leftNode){
        var marked = false;
        while (true){
            try{
                var head = Head;
                var headNext = head.Next.Value;

                while (true){
                    var res = SearchSchema(searchValue, ref leftNode, ref head, ref headNext, ref marked);
                    if (res is not null){
                        return res;
                    }
                }
            }
            catch{
                continue;
            }
        }
    }
}