using System.ServiceModel;
using Lab1.Utils;

namespace Lab1{
    public class SkipListLockFree<T>{
        private readonly Node<T> _head = new(int.MinValue);

        private readonly Node<T> _tail = new(int.MaxValue);

        public Node<T> Head => _head;
        public Node<T> Tail => _tail;

        public SkipListLockFree(){
            for (var i = 0; i < _head.Next.Length; ++i){
                _head.Next[i] = new MarkedReference<Node<T>>(_tail, false);
            }
        }

        private (Node<T>[], Node<T>[]) GetPredsAndSuccs(){
            var preds = new Node<T>[Config.MaxLevel + 1];
            var succs = new Node<T>[Config.MaxLevel + 1];

            return (succs, preds);
        }

        private void FillInNext(Node<T> node, Node<T>[] succs){
            for (var level = Config.MinLevel; level <= node.TopLevel; level++){
                var tempSucc = succs[level];
                node.Next[level] = new MarkedReference<Node<T>>(tempSucc, false);
            }
        }

        private bool IsComparedAndExchanged(Node<T> pred, int level, Node<T> node, Node<T> succ){
            return pred.Next[level].CompareAndExchange(node, false, succ, false);
        }

        private void IterateOverAllLevelsUp(Node<T> node, Node<T>[] preds, Node<T>[] succs){
            for (var level = Config.MinLevel + 1; level <= node.TopLevel; level++){
                while (true){
                    var pred = preds[level];
                    var succ = succs[level];

                    if (IsComparedAndExchanged(pred, level, node, succ)){
                        break;
                    }

                    Find(node, ref preds, ref succs);
                }
            }
        }

        public bool Insert(Node<T> node){
            var (succs, preds) = GetPredsAndSuccs();

            while (true){
                if (Find(node, ref preds, ref succs)){
                    return false;
                }
                
                FillInNext(node, succs);
                
                var pred = preds[Config.MinLevel];
                var succ = succs[Config.MinLevel];

                node.Next[Config.MinLevel] = new MarkedReference<Node<T>>(succ, false);

                if (!IsComparedAndExchanged(pred, Config.MinLevel, node, succ)){
                    continue;
                }
                
                IterateOverAllLevelsUp(node, preds, succs);
                
                return true;
            }
        }

        private Node<T> IterateOverAllLevelDown(ref Node<T> succ, Node<T> node){
            for (var level = node.TopLevel; level > Config.MinLevel; level--){
                var isMarked = false;
                succ = node.Next[level].Get(ref isMarked);

                while (!isMarked){
                    node.Next[level].CompareAndExchange(succ, true, succ, false);
                    succ = node.Next[level].Get(ref isMarked);
                }
            }

            return succ;
        }

        private bool RemoveSucc(Node<T> succ, Node<T> node, Node<T>[] succs, Node<T>[] preds, ref bool marked){
            while (true){
                var iMarkedIt = node.Next[Config.MinLevel].CompareAndExchange(succ, true, succ, false);
                succ = succs[Config.MinLevel].Next[Config.MinLevel].Get(ref marked);

                if (iMarkedIt){
                    Find(node, ref preds, ref succs);
                    return true;
                }

                if (marked){
                    return false;
                }
            }
        }

        public bool Delete(Node<T> node){
            var (succs, preds) = GetPredsAndSuccs();

            while (true){
                if (!Find(node, ref preds, ref succs)){
                    return false;
                }

                Node<T> succ = null;
                IterateOverAllLevelDown(ref succ, node);
                var marked = false;
                succ = node.Next[Config.MinLevel].Get(ref marked);

                return RemoveSucc(succ, node, succs, preds, ref marked);
            }
        }
        private void OnFind(int level, ref bool marked, ref Node<T> curr, ref Node<T> pred, Node<T> node, bool isRetryNeeded){
            while (true){
                var succ = curr.Next[level].Get(ref marked);
                while (marked){
                    if (!IsComparedAndExchanged(pred, level, succ, curr)){
                        throw new IsComparedNotFoundException("Not Found");
                    }
                    curr = pred.Next[level].Value;
                    succ = curr.Next[level].Get(ref marked);
                }

                if (curr.NodeKey < node.NodeKey){
                    pred = curr;
                    curr = succ;
                }

                else{
                    break;
                }           
            }
        }
        public bool Find(Node<T> node, ref Node<T>[] preds, ref Node<T>[] succs){
            var marked = false;
            var isRetryNeeded = false;
            Node<T> curr = null;

            while (true){
                try{
                    var pred = _head;
                    for (var level = Config.MaxLevel; level >= Config.MinLevel; level--){
                        curr = pred.Next[level].Value;

                        OnFind(level, ref marked, ref curr, ref pred, node, isRetryNeeded);

                        preds[level] = pred;
                        succs[level] = curr;
                    }

                    return curr != null && (curr.NodeKey == node.NodeKey);
                }
                catch{
                    continue;
                }
            }
        }
    }
}