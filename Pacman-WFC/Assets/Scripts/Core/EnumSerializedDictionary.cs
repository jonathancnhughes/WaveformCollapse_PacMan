using System;
using System.Collections.Generic;

namespace JFlex.Core
{
    public class EnumSerializedDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue> where TKey : Enum
    {
        public override void OnBeforeSerialize()
        {
            SyncWithEnum();
        }

        private void SyncWithEnum()
        {
            var enumValues = (TKey[])Enum.GetValues(typeof(TKey));

            // Initialize if empty
            if (keys.Count == 0)
            {
                foreach (var key in enumValues)
                {
                    keys.Add(key);
                    values.Add(default);
                }
                return;
            }

            // Add missing enum values
            foreach (var enumKey in enumValues)
            {
                if (!keys.Contains(enumKey))
                {
                    keys.Add(enumKey);
                    values.Add(default);
                }
            }

            // Remove stale enum values
            for (int i = keys.Count - 1; i >= 0; i--)
            {
                if (!Array.Exists(enumValues, e => EqualityComparer<TKey>.Default.Equals(e, keys[i])))
                {
                    keys.RemoveAt(i);
                    values.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            SyncWithEnum();
        }
#endif
    }
}