using System;
using System.Collections.Generic;

namespace Bayes
{
    public readonly struct Evidence
    {
        public string Var { get; }
        public string Value { get; }

        public Evidence(string var, string value)
        {
            Var = var ?? throw new ArgumentNullException(nameof(var));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static Evidence Of(string var, string value) => new Evidence(var, value);
    }

    public sealed class EvidenceSet
    {
        private readonly Dictionary<string, string> _values;

        private EvidenceSet(Dictionary<string, string> values)
        {
            _values = values;
        }

        public IReadOnlyDictionary<string, string> Values => _values;

        public static EvidenceSet Create(params Evidence[] items)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            if (items == null)
            {
                return new EvidenceSet(dict);
            }

            for (int i = 0; i < items.Length; i++)
            {
                var ev = items[i];
                if (dict.ContainsKey(ev.Var))
                {
                    throw new ArgumentException($"重复的证据变量: {ev.Var}");
                }
                dict[ev.Var] = ev.Value;
            }

            return new EvidenceSet(dict);
        }

        public bool TryGetValue(string varName, out string value) => _values.TryGetValue(varName, out value);
    }

    public sealed class Distribution
    {
        private readonly Dictionary<string, float> _values;

        public IReadOnlyDictionary<string, float> Values => _values;

        public float this[string value] => _values[value];

        public Distribution(IDictionary<string, float> values, bool normalize = true)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("分布不能为空", nameof(values));
            }

            var temp = new Dictionary<string, float>(values.Count, StringComparer.Ordinal);
            float sum = 0f;
            foreach (var kv in values)
            {
                if (kv.Key == null)
                {
                    throw new ArgumentException("分布包含空的取值名");
                }
                if (kv.Value < 0f)
                {
                    throw new ArgumentException("分布概率不能为负数");
                }
                temp[kv.Key] = kv.Value;
                if (normalize)
                {
                    sum += kv.Value;
                }
            }

            if (normalize)
            {
                if (sum <= 0f)
                {
                    throw new ArgumentException("分布总和必须大于0");
                }

                var keys = new List<string>(temp.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys[i];
                    temp[key] = temp[key] / sum;
                }
            }

            _values = temp;
        }

        public static Distribution Uniform(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("取值列表不能为空", nameof(values));
            }

            float p = 1f / values.Count;
            var dict = new Dictionary<string, float>(values.Count, StringComparer.Ordinal);
            for (int i = 0; i < values.Count; i++)
            {
                dict[values[i]] = p;
            }
            return new Distribution(dict, normalize: false);
        }
    }

    public sealed class Cpt
    {
        private readonly Dictionary<ParentsKey, Distribution> _table;

        public IReadOnlyDictionary<ParentsKey, Distribution> Table => _table;

        public Cpt(IReadOnlyDictionary<ParentsKey, Distribution> table)
        {
            if (table == null || table.Count == 0)
            {
                throw new ArgumentException("CPT 不能为空", nameof(table));
            }
            _table = new Dictionary<ParentsKey, Distribution>(table);
        }

        public bool TryGetDistribution(ParentsKey key, out Distribution distribution) => _table.TryGetValue(key, out distribution);

        public void Set(ParentsKey key, Distribution distribution)
        {
            _table[key] = distribution ?? throw new ArgumentNullException(nameof(distribution));
        }
    }

    public readonly struct ParentsKey : IEquatable<ParentsKey>
    {
        private static readonly ParentsKey EmptyKey = new ParentsKey(Array.Empty<(string Var, string Value)>());

        private readonly (string Var, string Value)[] _pairs;
        private readonly int _hash;

        public IReadOnlyList<(string Var, string Value)> Pairs => _pairs;
        public static ParentsKey Empty => EmptyKey;

        public ParentsKey(IEnumerable<(string Var, string Value)> pairs)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            var list = new List<(string Var, string Value)>();
            foreach (var pair in pairs)
            {
                if (pair.Var == null || pair.Value == null)
                {
                    throw new ArgumentException("父节点键包含空值");
                }
                list.Add(pair);
            }

            list.Sort((a, b) => string.CompareOrdinal(a.Var, b.Var));
            _pairs = list.ToArray();

            unchecked
            {
                int hash = 17;
                for (int i = 0; i < _pairs.Length; i++)
                {
                    hash = (hash * 31) + _pairs[i].Var.GetHashCode();
                    hash = (hash * 31) + _pairs[i].Value.GetHashCode();
                }
                _hash = hash;
            }
        }

        public static ParentsKey FromPairs(params (string Var, string Value)[] pairs)
        {
            return new ParentsKey(pairs ?? Array.Empty<(string Var, string Value)>());
        }

        public bool Equals(ParentsKey other)
        {
            if (_hash != other._hash)
            {
                return false;
            }
            if (_pairs == null && other._pairs == null)
            {
                return true;
            }
            if (_pairs == null || other._pairs == null)
            {
                return false;
            }
            if (_pairs.Length != other._pairs.Length)
            {
                return false;
            }
            for (int i = 0; i < _pairs.Length; i++)
            {
                if (!string.Equals(_pairs[i].Var, other._pairs[i].Var, StringComparison.Ordinal))
                {
                    return false;
                }
                if (!string.Equals(_pairs[i].Value, other._pairs[i].Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj) => obj is ParentsKey other && Equals(other);
        public override int GetHashCode() => _hash;
        public static bool operator ==(ParentsKey lhs, ParentsKey rhs) => lhs.Equals(rhs);
        public static bool operator !=(ParentsKey lhs, ParentsKey rhs) => !lhs.Equals(rhs);
    }

    public sealed class Sample
    {
        private readonly Dictionary<string, string> _values;

        public IReadOnlyDictionary<string, string> Values => _values;

        public Sample(IDictionary<string, string> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("样本不能为空", nameof(values));
            }

            _values = new Dictionary<string, string>(values.Count, StringComparer.Ordinal);
            foreach (var kv in values)
            {
                if (kv.Key == null || kv.Value == null)
                {
                    throw new ArgumentException("样本包含空的变量名或取值");
                }
                _values[kv.Key] = kv.Value;
            }
        }

        public bool TryGetValue(string varName, out string value) => _values.TryGetValue(varName, out value);
    }
}
