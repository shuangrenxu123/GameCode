using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HTN;
using UnityEngine;

namespace AIBlackboard
{
    public readonly struct BlackboardKey<T> : IEquatable<BlackboardKey<T>>
    {
        public readonly int Id;


        public BlackboardKey(string name)
        {
            unchecked { Id = (name.GetHashCode() * 397) ^ typeof(T).GetHashCode(); }
        }

        public BlackboardKey(int id)
        {
            unchecked { Id = (id * 397) ^ typeof(T).GetHashCode(); }
        }

        public BlackboardKey(int rawKeyHash, Type keyType)
        {
            unchecked
            {
                int h1 = rawKeyHash;
                int h2 = keyType.GetHashCode();
                // 混合 KeyHash, KeyTypeHash, ValueTypeHash(T)
                Id = (h1 * 397) ^ (h2 * 17) ^ typeof(T).GetHashCode();
            }
        }

        //在需要的时候隐式转换
        public static implicit operator BlackboardKey<T>(string name) => new BlackboardKey<T>(name);
        public static implicit operator BlackboardKey<T>(int id) => new BlackboardKey<T>(id);

        public bool Equals(BlackboardKey<T> other) => Id == other.Id;
        public override bool Equals(object obj) => obj is BlackboardKey<T> other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(BlackboardKey<T> lhs, BlackboardKey<T> rhs) => lhs.Id == rhs.Id;
        public static bool operator !=(BlackboardKey<T> lhs, BlackboardKey<T> rhs) => lhs.Id != rhs.Id;

    }
    internal abstract class BlackboardEntry
    { }

    internal sealed class BlackboardEntry<T> : BlackboardEntry
    {
        private T _value;
        private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;
        public event Action<T> OnValueChanged;

        public Type ValueType => typeof(T);

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!Comparer.Equals(_value, value))
                {
                    _value = value;
                    OnValueChanged?.Invoke(_value);
                }
            }
        }

        public BlackboardEntry(T initialValue) => _value = initialValue;
    }

    /// <summary>
    /// 单个AI的数据存储。
    /// </summary>
    public class Blackboard
    {
        //这里的T用int 是为了泛型约束
        Dictionary<int, BlackboardEntry> data = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue<T>(BlackboardKey<T> key, T value)
        {
            SetInternal(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue<T>(BlackboardKey<T> key, T defaultValue = default)
        {
            return GetInternal(key, defaultValue);
        }

        public bool TryGetValue<T>(BlackboardKey<T> key, out T value)
        {
            return TryGetInternal(key, out value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue<TKey, TVal>(TKey rawKey, TVal value)
        {
            // 帮你 New 一个 Key，利用 EqualityComparer 避免装箱
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            var key = new BlackboardKey<TVal>(hash, typeof(TKey));
            SetInternal(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TVal GetValue<TKey, TVal>(TKey rawKey, TVal defaultValue = default)
        {
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            var key = new BlackboardKey<TVal>(hash, typeof(TKey));
            return GetInternal(key, defaultValue);
        }

        public bool TryGetValue<TKey, TVal>(TKey rawKey, out TVal value)
        {
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            var key = new BlackboardKey<TVal>(hash, typeof(TKey));
            return TryGetInternal(key, out value);
        }
        private void SetInternal<T>(BlackboardKey<T> key, T value)
        {
            if (data.TryGetValue(key.Id, out var entryBase))
            {

                ((BlackboardEntry<T>)entryBase).Value = value;

            }
            else
            {
                data[key.Id] = new BlackboardEntry<T>(value);
            }
        }

        private T GetInternal<T>(BlackboardKey<T> key, T defaultValue)
        {
            if (data.TryGetValue(key.Id, out var entryBase))
            {

                return ((BlackboardEntry<T>)entryBase).Value;
            }
            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsData<T>(BlackboardKey<T> key)
        {
            return data.ContainsKey(key.Id);
        }


        public void Subscribe<T>(BlackboardKey<T> key, Action<T> callback)
        {
            if (!data.TryGetValue(key.Id, out var entryBase))
            {
                var newEntry = new BlackboardEntry<T>(default);
                data[key.Id] = newEntry;
                entryBase = newEntry;
            }
                   // 安全强转，这里可以用 Unsafe.As 但对于 Subscribe 频率不高，安全第一
                   ((BlackboardEntry<T>)entryBase).OnValueChanged += callback;
        }

        public void Unsubscribe<T>(BlackboardKey<T> key, Action<T> callback)
        {
            if (data.TryGetValue(key.Id, out var entryBase))
            {
                ((BlackboardEntry<T>)entryBase).OnValueChanged -= callback;
            }
        }

        // 泛型重载
        public void Subscribe<TKey, TVal>(TKey rawKey, Action<TVal> callback)
        {
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            Subscribe(new BlackboardKey<TVal>(hash, typeof(TKey)), callback);
        }

        public void Unsubscribe<TKey, TVal>(TKey rawKey, Action<TVal> callback)
        {
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            Unsubscribe(new BlackboardKey<TVal>(hash, typeof(TKey)), callback);
        }


        /// <summary>
        /// 判断是否有TKey的类型，他的Value类型为TVal
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="rawKey"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsData<TKey, TVal>(TKey rawKey)
        {
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            // 注意：这里必须手动构造一次 Key 来获取完整的混合 Hash
            // 必须传入 typeof(TVal)，因为 Key 的 Hash 算法里包含了 Value 的类型
            var key = new BlackboardKey<TVal>(hash, typeof(TKey));
            return data.ContainsKey(key.Id);
        }
        private bool TryGetInternal<T>(BlackboardKey<T> key, out T value)
        {
            if (data.TryGetValue(key.Id, out var entryBase))
            {
                value = ((BlackboardEntry<T>)entryBase).Value;
                return true;
            }
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<T>(BlackboardKey<T> key) => data.Remove(key.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<TKey, TVal>(TKey rawKey)
        {
            int hash = EqualityComparer<TKey>.Default.GetHashCode(rawKey);
            var key = new BlackboardKey<TVal>(hash, typeof(TKey));
            return data.Remove(key.Id);
        }

        public void Reset()
        {
            data.Clear();
        }

    }
}