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

        /*
            Якщо знайдено вузол із таким самим значенням, що й значення, яке потрібно вставити, тоді
             нічого не слід робити (математичний набір не може містити дублікатів).
             В іншому випадку ми повинні створити новий вузол і вставити його в список.
        */
        public bool Insert(Node<T> node){
            var (succs, preds) = GetPredsAndSuccs();

            while (true){
                // шукаємо елемент із таким ж значенням, а також заповнюємо попередників і наступників
                if (Find(node, ref preds, ref succs)){
                    return false;
                }

                /*
                    Ітеруємся по всіх рівнях і заповнюємо всі Next для створеної ноди за допомогою succs
                */
                FillInNext(node, succs);

                /*
                 Наступним кроком є спроба додати новий вузол, зв’язавши його зі списком нижнього рівня між вузлами preds[0] і succs[0], які повертає find().
                */
                var pred = preds[Config.MinLevel];
                var succ = succs[Config.MinLevel];

                node.Next[Config.MinLevel] = new MarkedReference<Node<T>>(succ, false);

                /*
                 * перевіряємо чи ці Node все ще посилаються один на одного і не були вилучені зі списку
                 */
                if (!IsComparedAndExchanged(pred, Config.MinLevel, node, succ)){
                    continue;
                }
                
                /*
                 * Для кожного рівня намагаємся з’єднати Node, встановивши попередника, якщо він посилається на дійсного наступника
                 * У разі успіху викликаємо break і переходить на наступний рівень.
                 * Якщо це не вдалося, то вузол, на який посилається попередник, повинен бути змінений, і find() викликається знову,
                 * щоб знайти новий дійсний набір попередників і наступників. Ми відкидаємо результат виклику find(),
                 * тому що ми дбаємо лише про повторне обчислення попередників і наступників для решти
                 * незв'язаних рівнів. Після того, як усі рівні пов’язані, метод повертає true
                 */
                
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
                // викликаємо Find щоб перевірити чи існує така Node та шукаємо попередників та наступників
                if (!Find(node, ref preds, ref succs)){
                    return false;
                }
                
                /*
                 * логічно видаляємо пов’язаний ключ із абстрактного набору та готує його до фізичного видалення
                 * Ітеруємся починаючи з верхнього рівня і до останнього (не включаючи) і використовуємо CompareAndExchange для Next.
                 * Якщо виявляється, що посилання позначене (або тому, що воно вже було позначено, або тому, що спроба була успішною),
                 * метод переходить до посилання наступного рівня.
                 * В іншому випадку посилання поточного рівня перечитується, оскільки воно повинно бути змінено іншим паралельним потоком, тому спробу позначення потрібно повторити.
                 */
                Node<T> succ = null;
                IterateOverAllLevelDown(ref succ, node);
                var marked = false;
                succ = node.Next[Config.MinLevel].Get(ref marked);
                /*
                 * Власне видалення Node
                 * позначаємо Next поле за допомогою compareAndSet().
                 * Перш ніж повернути true, метод find() викликається знову.
                 * Цей виклик є оптимізацією: як побічний ефект, find() фізично видаляє всі посилання на вузол, який він шукає, якщо цей вузол уже логічно видалено
                 */
                return RemoveSucc(succ, node, succs, preds, ref marked);
            }
        }

        private void OnFind(int level, ref bool marked, ref Node<T> curr, ref Node<T> pred, Node<T> node,
            bool isRetryNeeded){
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