﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lab1{
    internal sealed class CollectionReadWritePerformance{
        private const int MAX_VALUE = 100000000;
        private readonly SkipListLockFree<int> _target;
        private readonly Thread[] _threads;
        private readonly int _iterations;
        private readonly Random rand = new Random();

        public CollectionReadWritePerformance(SkipListLockFree<int> target, int readersCount, int writersCount, int iterations){
            _target = target;
            _iterations = iterations;
            var count = writersCount + readersCount;
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
                    var random = rand.Next(MAX_VALUE);
                    var node = new Node<int>(random, i);
                    _target.Insert(node);
                }
            }
            catch (Exception ex){
                Console.WriteLine(ex);
            }
        }
    }
}