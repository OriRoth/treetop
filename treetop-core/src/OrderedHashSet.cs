using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace treetop
{
    /// <summary>
    /// Hash set that respects insertion order.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class OrderedHashSet<T> : KeyedCollection<T, T>
    {
        protected override T GetKeyForItem(T item)
        {
            return item;
        }
        public new void Add(T item)
        {
            if (!Contains(item))
                base.Add(item);
        }
        /// <summary>
        /// Checks for set equality.
        /// </summary>
        /// <param name="other">other set</param>
        /// <returns>whether set contains same items as this</returns>
        public bool SetEquals(OrderedHashSet<T> other)
        {
            ISet<T> set1 = new HashSet<T>(), set2 = new HashSet<T>();
            foreach (T item in this)
                set1.Add(item);
            foreach (T item in other)
                set2.Add(item);
            return set1.SetEquals(set2);
        }
        /// <summary>
        /// Computes the power set of this set, i.e., the set of all sub-sets.
        /// </summary>
        /// <returns>power set</returns>
        public OrderedHashSet<OrderedHashSet<T>> PowerSet()
        {
            int n = Count;
            int pn = 1 << n;
            OrderedHashSet<OrderedHashSet<T>> powerSet = new OrderedHashSet<OrderedHashSet<T>>();
            for (int mask = 0; mask < pn; mask++)
            {
                OrderedHashSet<T> set = new OrderedHashSet<T>();
                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) > 0)
                    {
                        set.Add(this[i]);
                    }
                }
                powerSet.Add(set);
            }
            return powerSet;
        }
    }
}
