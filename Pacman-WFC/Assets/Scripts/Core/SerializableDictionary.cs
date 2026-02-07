using System;
using System.Collections.Generic;
using UnityEngine;

namespace JFlex.Core
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField] protected List<TKey> keys = new();
        [SerializeField] protected List<TValue> values = new();

        private Dictionary<TKey, TValue> dictionary = new();

        public Dictionary<TKey, TValue> Dictionary => dictionary;

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public virtual void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public virtual void OnAfterDeserialize()
        {
            dictionary = new Dictionary<TKey, TValue>();

            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                if (!dictionary.ContainsKey(keys[i]))
                    dictionary.Add(keys[i], values[i]);
            }
        }
    }
}