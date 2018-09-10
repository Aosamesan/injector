using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Injector.ContextLoader.HelperClass
{
    public class SimplePriorityQueue<TKey, TValue> where TKey : IComparable<TKey> where TValue : class
    {
        private IDictionary<TKey, Queue<TValue>> innerSortedDictionary;
        public int Count { get; private set; }
        public bool IsEmpty => Count == 0;

        public SimplePriorityQueue()
        {
            innerSortedDictionary = new SortedList<TKey, Queue<TValue>>(Comparer<TKey>.Default);
            Count = 0;
        }

        public TValue Dequeue()
        {
            var queue = innerSortedDictionary.First();
            if (queue.Value.Count == 1)
            {
                innerSortedDictionary.Remove(queue.Key);
            }

            Count--;
            return queue.Value.Dequeue();
        }

        public void Enqueue(TKey key, TValue item)
        {
            if (!innerSortedDictionary.ContainsKey(key))
            {
                innerSortedDictionary[key] = new Queue<TValue>();
            }
            innerSortedDictionary[key].Enqueue(item);
            Count++;
        }
    }
}