using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lab1{
    internal sealed class CollectionWritePerformanceSkipList{
        private const int MAX_VALUE = 10000;
        private readonly SkipListLockFree<int> _target;
        private readonly Thread[] _threads;
        private readonly int _iterations;
        private readonly object obj = new();
        private readonly Random rand = new();
        public SynchronizedCollection<Node<int>> SavedValue{ get; } = new();

        public CollectionWritePerformanceSkipList(SkipListLockFree<int> target, int writersCount, int iterations){
            _target = target;
            _iterations = iterations;
            _threads = new Thread[writersCount];

            for (var i = 0; i < writersCount; i++){
                _threads[i] = new Thread(Writer);
            }
        }

        public TimeSpan Run(){
            var watcher = new Stopwatch();
            watcher.Start();
            foreach (Thread t in _threads){
                t.Start();
            }

            foreach (Thread t in _threads){
                t.Join();
            }

            watcher.Stop();
            return watcher.Elapsed;
        }

        private void Writer(){
            try{
                for (var i = 0; i < _iterations; i++){
                    int random1, random2;
                    lock (obj){
                        random1 = rand.Next(MAX_VALUE);
                        random2 = rand.Next(MAX_VALUE);
                    }
                    var node = new Node<int>(random1, random2);
                    var res = _target.Insert(node);
                    if (res){
                        SavedValue.Add(node);
                    }
                }
            }
            catch (Exception ex){
                Console.WriteLine(ex);
            }
        }
    }
}