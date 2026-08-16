using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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



        public bool Equals(BlackboardKey<T> other) => Id == other.Id;
        public override bool Equals(object obj) => obj is BlackboardKey<T> other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(BlackboardKey<T> lhs, BlackboardKey<T> rhs) => lhs.Id == rhs.Id;
        public static bool operator !=(BlackboardKey<T> lhs, BlackboardKey<T> rhs) => lhs.Id != rhs.Id;

    }
    internal abstract class BlackboardEntry
    {
        public abstract Type ValueType { get; }

        /// <summary>
        /// 比较两个 Entry 的值是否相等；类型不同一律返回 false。
        /// </summary>
        public abstract bool ValueEquals(BlackboardEntry other);

        /// <summary>
        /// 取值的哈希，供 GOAP 等需要按条件散列的使用方使用。
        /// </summary>
        public abstract int GetValueHash();

        /// <summary>
        /// 从同类型 Entry 拷贝值（走 Value setter，会触发 OnValueChanged）。
        /// </summary>
        public abstract void CopyValueFrom(BlackboardEntry source);

        /// <summary>
        /// 复制一个只含值、不含订阅者的新 Entry。
        /// </summary>
        public abstract BlackboardEntry Clone();
    }

    internal sealed class BlackboardEntry<T> : BlackboardEntry
    {
        private T _value;
        private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;
        public event Action<T> OnValueChanged;

        public override Type ValueType => typeof(T);

        public override bool ValueEquals(BlackboardEntry other)
        {
            return other is BlackboardEntry<T> typed && Comparer.Equals(_value, typed._value);
        }

        public override int GetValueHash()
        {
            return Comparer.GetHashCode(_value);
        }

        public override void CopyValueFrom(BlackboardEntry source)
        {
            if (source is BlackboardEntry<T> typed)
            {
                Value = typed._value;
            }
        }

        public override BlackboardEntry Clone()
        {
            return new BlackboardEntry<T>(_value);
        }

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

        public int Count => data.Count;

        /// <summary>
        /// 供同程序集内（如 GOAP 规划器）直接遍历条目；外部程序集请使用类型化读写接口。
        /// </summary>

        internal Dictionary<int, BlackboardEntry> Entries => data;
        internal bool TryGetEntry(int keyId, out BlackboardEntry entry)
        {
            return data.TryGetValue(keyId, out entry);
        }

        /// <summary>
        /// 把 source 的值写入指定 key：已存在同类型条目时复用原条目并触发
        /// OnValueChanged；否则克隆出新条目存入，避免与来源方共享可变对象。
        /// </summary>
        internal void WriteEntry(int keyId, BlackboardEntry source)
        {
            if (data.TryGetValue(keyId, out var existing) &&
                existing.ValueType == source.ValueType)
            {
                existing.CopyValueFrom(source);
            }
            else
            {
                data[keyId] = source.Clone();
            }
        }

        /// <summary>
        /// 用指定条目整体替换该 key（不原地改原条目）：供规划器等需要"替换式写入"
        /// （零拷贝、不改旧引用）的场景。与 WriteEntry 的区别：WriteEntry 是"把值写进
        /// 已有条目"，ReplaceEntry 是"换掉整个条目对象"。
        /// </summary>
        internal void ReplaceEntry(int keyId, BlackboardEntry entry)
        {
            data[keyId] = entry;
        }

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

        /// <summary>
        /// 深拷贝一份黑板（条目值拷贝，不含订阅者）。
        /// 供规划回溯等需要快照/回滚的场景使用。
        /// </summary>
        public Blackboard Clone()
        {
            Blackboard clone = new Blackboard();
            foreach (KeyValuePair<int, BlackboardEntry> kv in data)
            {
                clone.data[kv.Key] = kv.Value.Clone();
            }
            return clone;
        }

    }
}