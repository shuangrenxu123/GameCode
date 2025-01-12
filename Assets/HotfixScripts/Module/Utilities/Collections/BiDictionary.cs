using System;
using System.Collections;
using System.Collections.Generic;

namespace Utilities.Collections
{
    /// <summary>
    /// 一个双向对应的字典
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class BiDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        public Dictionary<K, V> k2v { get; private set; }
        public Dictionary<V, K> v2k { get; private set; }
        public BiDictionary(int capacity = 4)
        {
            k2v = new(capacity);
            v2k = new(capacity);
        }

        public void Add(V value, K key) => Add(key, value);
        public void Add(K key, V value)
        {
            if (k2v.ContainsKey(key) || v2k.ContainsKey(value))
            {
                throw new ArgumentException();
            }

            k2v.Add(key, value);
            v2k.Add(value, key);

        }

        public void Remove(K key)
        {
            if (!k2v.TryGetValue(key, out V value))
            {
                throw new KeyNotFoundException();
            }
            k2v.Remove(key);
            v2k.Remove(value);
        }
        public void Remove(V key)
        {

            if (!v2k.TryGetValue(key, out K value))
            {
                throw new KeyNotFoundException();
            }
            v2k.Remove(key);
            k2v.Remove(value);
        }

        public V this[K key]
        {
            get
            {
                if (!k2v.TryGetValue(key, out V oldValue))
                {
                    throw new KeyNotFoundException();
                }
                return oldValue;
            }
            set
            {
                if (k2v.TryGetValue(key, out V oldValue))
                {
                    v2k.Remove(oldValue);

                    if (v2k.TryGetValue(value, out var v2))
                    {
                        k2v.Remove(v2);
                        v2k.Remove(value);
                    }

                    v2k.Add(value, key);
                    k2v[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }
        public K this[V key]
        {
            get
            {
                if (!v2k.TryGetValue(key, out var oldValue))
                {
                    throw new KeyNotFoundException();
                }
                return oldValue;
            }
            set
            {
                if (v2k.TryGetValue(key, out var oldValue))
                {
                    k2v.Remove(oldValue);
                    if (k2v.TryGetValue(value, out var v2))
                    {
                        v2k.Remove(v2);
                        k2v.Remove(value);
                    }
                    k2v.Add(value, key);
                    v2k[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public bool ContainsKey(K key)
        {
            return k2v.ContainsKey(key);
        }

        public bool ContainsKey(V key)
        {
            return v2k.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            foreach (var key in k2v)
            {
                yield return key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
