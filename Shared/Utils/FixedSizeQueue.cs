using System.Collections;
using System.Collections.Generic;

namespace Shared.Utils
{
    public class FixedSizedQueue<T> : IEnumerable<T>
    {
        Queue<T> q = new Queue<T>();

        public FixedSizedQueue(int limit)
        {
            Limit = limit;
        }

        public int Limit { get; set; }
        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            T overflow;
            while (q.Count > Limit)
            {
                overflow = q.Dequeue();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return q.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return q.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return q.Count;
            }
        }
    }
}
