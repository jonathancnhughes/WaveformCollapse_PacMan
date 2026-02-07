using System.Collections.Generic;
using UnityEngine;

namespace JFlex.Core
{
    [System.Serializable]
    public struct WeightedRandomItem<T>
    {
        public int Weight;
        public T Item;
    }

    [System.Serializable]
    public class WeightedRandomTable<T>
    {
        public List<WeightedRandomItem<T>> WeightedItems;

        public List<T> Items
        {
            get
            {
                var items = new List<T>();
                foreach (var wi in WeightedItems)
                {
                    items.Add(wi.Item);
                }

                return items;
            }
        }

        public int Count => WeightedItems.Count;

        private int TotalWeight
        {
            get
            {
                var value = 0;
                foreach (var i in WeightedItems)
                {
                    value += i.Weight;
                }

                return value;
            }
        }

        public void RemoveFromTable(T itemToRemove)
        {
            for (var i = 0; i < WeightedItems.Count; i++)
            {
                var item = WeightedItems[i].Item;
                if (Object.Equals(item, itemToRemove))
                {
                    WeightedItems.RemoveAt(i);
                    break;
                }
            }
        }

        public T GetRandomItem()
        {
            var random = Random.Range(0f, 1f) * TotalWeight;
            var cumulative = 0f;

            foreach (var item in WeightedItems)
            {
                cumulative += item.Weight;

                if (cumulative > random)
                {
                    return item.Item;
                }
            }

            return WeightedItems[^1].Item;
        }
    }
}