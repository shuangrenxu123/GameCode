using System;
using System.Collections.Generic;

namespace Bayes
{
    public sealed class BayesContext : IBayesContext, IBayesQuery, IBayesUpdate
    {
        private sealed class VariableDef
        {
            public string Name { get; }
            public IReadOnlyList<string> Values { get; }
            public Dictionary<string, int> ValueIndex { get; }

            public VariableDef(string name, IReadOnlyList<string> values, Dictionary<string, int> valueIndex)
            {
                Name = name;
                Values = values;
                ValueIndex = valueIndex;
            }
        }

        private sealed class Node
        {
            public string Name { get; }
            public List<string> Parents { get; set; }
            public Distribution Prior { get; set; }
            public Cpt Conditional { get; set; }

            public Node(string name)
            {
                Name = name;
                Parents = new List<string>(0);
            }
        }

        private readonly Dictionary<string, VariableDef> _variables = new Dictionary<string, VariableDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, Node> _nodes = new Dictionary<string, Node>(StringComparer.Ordinal);
        private List<string> _topoOrder = new List<string>();
        private bool _topoDirty = true;

        public static BayesContext Create() => new BayesContext();

        public IBayesQuery Query => this;
        public IBayesUpdate Update => this;

        public void DefineVariable(string name, IReadOnlyList<string> values)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("变量名不能为空", nameof(name));
            }
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("变量取值不能为空", nameof(values));
            }
            if (_variables.ContainsKey(name))
            {
                throw new InvalidOperationException($"变量已存在: {name}");
            }

            var list = new List<string>(values.Count);
            var index = new Dictionary<string, int>(values.Count, StringComparer.Ordinal);
            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("变量取值不能为空字符串");
                }
                if (index.ContainsKey(value))
                {
                    throw new ArgumentException($"变量 {name} 的取值重复: {value}");
                }
                index[value] = i;
                list.Add(value);
            }

            var def = new VariableDef(name, list, index);
            _variables[name] = def;
            _nodes[name] = new Node(name);
            _topoDirty = true;
        }

        public void SetPrior(string varName, Distribution prior)
        {
            var def = GetVariable(varName);
            if (prior == null)
            {
                throw new ArgumentNullException(nameof(prior));
            }

            var node = _nodes[varName];
            if (node.Parents.Count > 0)
            {
                throw new InvalidOperationException($"变量 {varName} 已设置父节点，不能再设置先验");
            }

            ValidateDistribution(def, prior);
            node.Prior = prior;
            node.Conditional = null;
        }

        public void SetConditional(string childVar, IReadOnlyList<string> parentVars, Cpt table)
        {
            var childDef = GetVariable(childVar);
            if (parentVars == null || parentVars.Count == 0)
            {
                throw new ArgumentException("父节点列表不能为空", nameof(parentVars));
            }
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var parents = new List<string>(parentVars.Count);
            var parentSet = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < parentVars.Count; i++)
            {
                string parent = parentVars[i];
                if (string.IsNullOrWhiteSpace(parent))
                {
                    throw new ArgumentException("父节点名不能为空");
                }
                if (string.Equals(parent, childVar, StringComparison.Ordinal))
                {
                    throw new ArgumentException("父节点不能与子节点相同");
                }
                if (!_variables.ContainsKey(parent))
                {
                    throw new InvalidOperationException($"父节点未定义: {parent}");
                }
                if (!parentSet.Add(parent))
                {
                    throw new ArgumentException($"父节点重复: {parent}");
                }
                parents.Add(parent);
            }

            ValidateCpt(childDef, parents, table);

            var node = _nodes[childVar];
            node.Parents = parents;
            node.Conditional = table;
            node.Prior = null;
            _topoDirty = true;
        }

        public void FitFromSamples(IEnumerable<Sample> samples, float smoothing = 0f)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }
            if (smoothing < 0f)
            {
                throw new ArgumentException("平滑系数不能小于0", nameof(smoothing));
            }

            var nodeList = new List<Node>(_nodes.Values);
            var parentKeysByVar = new Dictionary<string, List<ParentsKey>>(StringComparer.Ordinal);
            var countsByVar = new Dictionary<string, Dictionary<ParentsKey, float[]>>(StringComparer.Ordinal);

            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                var def = GetVariable(node.Name);
                var parentKeys = BuildParentKeys(node.Parents);
                parentKeysByVar[node.Name] = parentKeys;

                var counts = new Dictionary<ParentsKey, float[]>(parentKeys.Count);
                for (int k = 0; k < parentKeys.Count; k++)
                {
                    counts[parentKeys[k]] = new float[def.Values.Count];
                }
                countsByVar[node.Name] = counts;
            }

            foreach (var sample in samples)
            {
                if (sample == null)
                {
                    continue;
                }

                for (int i = 0; i < nodeList.Count; i++)
                {
                    var node = nodeList[i];
                    var def = _variables[node.Name];

                    if (!sample.TryGetValue(node.Name, out var varValue))
                    {
                        continue;
                    }
                    if (!def.ValueIndex.ContainsKey(varValue))
                    {
                        continue;
                    }

                    ParentsKey key;
                    if (node.Parents.Count == 0)
                    {
                        key = ParentsKey.Empty;
                    }
                    else
                    {
                        var pairs = new (string Var, string Value)[node.Parents.Count];
                        bool ok = true;
                        for (int p = 0; p < node.Parents.Count; p++)
                        {
                            string parent = node.Parents[p];
                            if (!sample.TryGetValue(parent, out var parentValue))
                            {
                                ok = false;
                                break;
                            }
                            var parentDef = _variables[parent];
                            if (!parentDef.ValueIndex.ContainsKey(parentValue))
                            {
                                ok = false;
                                break;
                            }
                            pairs[p] = (parent, parentValue);
                        }
                        if (!ok)
                        {
                            continue;
                        }
                        key = new ParentsKey(pairs);
                    }

                    var counts = countsByVar[node.Name][key];
                    counts[def.ValueIndex[varValue]] += 1f;
                }
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                var def = _variables[node.Name];
                var parentKeys = parentKeysByVar[node.Name];
                var counts = countsByVar[node.Name];

                if (node.Parents.Count == 0)
                {
                    var prior = BuildDistribution(def, counts[ParentsKey.Empty], smoothing, node.Prior);
                    node.Prior = prior;
                    node.Conditional = null;
                }
                else
                {
                    var table = new Dictionary<ParentsKey, Distribution>(parentKeys.Count);
                    for (int k = 0; k < parentKeys.Count; k++)
                    {
                        var key = parentKeys[k];
                        Distribution fallback = null;
                        if (node.Conditional != null && node.Conditional.TryGetDistribution(key, out var exist))
                        {
                            fallback = exist;
                        }
                        var dist = BuildDistribution(def, counts[key], smoothing, fallback);
                        table[key] = dist;
                    }
                    node.Conditional = new Cpt(table);
                    node.Prior = null;
                }
            }

            _topoDirty = true;
        }

        public float Probability(string targetVar, string targetValue, EvidenceSet evidence)
        {
            var dist = Marginal(targetVar, evidence);
            if (!dist.Values.TryGetValue(targetValue, out var p))
            {
                throw new ArgumentException($"变量 {targetVar} 不存在取值 {targetValue}");
            }
            return p;
        }

        public Distribution Marginal(string targetVar, EvidenceSet evidence)
        {
            var targetDef = GetVariable(targetVar);
            ValidateReady();
            EnsureTopologicalOrder();

            var evidenceMap = BuildEvidenceMap(evidence);

            var result = new Dictionary<string, float>(targetDef.Values.Count, StringComparer.Ordinal);
            double total = 0d;

            for (int i = 0; i < targetDef.Values.Count; i++)
            {
                string value = targetDef.Values[i];
                if (evidenceMap.TryGetValue(targetVar, out var locked) &&
                    !string.Equals(locked, value, StringComparison.Ordinal))
                {
                    result[value] = 0f;
                    continue;
                }

                var assignment = new Dictionary<string, string>(evidenceMap, StringComparer.Ordinal)
                {
                    [targetVar] = value
                };

                double p = EnumerateAll(0, _topoOrder, assignment);
                result[value] = (float)p;
                total += p;
            }

            if (total <= 0d)
            {
                return Distribution.Uniform(targetDef.Values);
            }

            var keys = new List<string>(result.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                result[key] = (float)(result[key] / total);
            }

            return new Distribution(result, normalize: false);
        }

        public string MAP(string targetVar, EvidenceSet evidence)
        {
            var dist = Marginal(targetVar, evidence);
            string bestValue = null;
            float best = float.MinValue;
            foreach (var kv in dist.Values)
            {
                if (kv.Value > best)
                {
                    best = kv.Value;
                    bestValue = kv.Key;
                }
            }
            return bestValue;
        }

        private VariableDef GetVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("变量名不能为空", nameof(name));
            }
            if (!_variables.TryGetValue(name, out var def))
            {
                throw new InvalidOperationException($"变量未定义: {name}");
            }
            return def;
        }

        private void ValidateReady()
        {
            foreach (var kv in _nodes)
            {
                var node = kv.Value;
                if (node.Parents.Count == 0)
                {
                    if (node.Prior == null)
                    {
                        throw new InvalidOperationException($"变量 {node.Name} 未设置先验分布");
                    }
                }
                else
                {
                    if (node.Conditional == null)
                    {
                        throw new InvalidOperationException($"变量 {node.Name} 未设置条件概率表");
                    }
                }
            }
        }

        private void ValidateDistribution(VariableDef def, Distribution dist)
        {
            if (dist.Values.Count != def.Values.Count)
            {
                throw new ArgumentException($"变量 {def.Name} 的分布取值数量不匹配");
            }
            for (int i = 0; i < def.Values.Count; i++)
            {
                string value = def.Values[i];
                if (!dist.Values.ContainsKey(value))
                {
                    throw new ArgumentException($"变量 {def.Name} 的分布缺少取值: {value}");
                }
            }
        }

        private void ValidateCpt(VariableDef childDef, IReadOnlyList<string> parents, Cpt cpt)
        {
            foreach (var kv in cpt.Table)
            {
                var key = kv.Key;
                var dist = kv.Value;
                ValidateDistribution(childDef, dist);

                if (key.Pairs == null || key.Pairs.Count != parents.Count)
                {
                    throw new ArgumentException("CPT 父节点组合数量不匹配");
                }

                var used = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < key.Pairs.Count; i++)
                {
                    var pair = key.Pairs[i];
                    if (!used.Add(pair.Var))
                    {
                        throw new ArgumentException("CPT 父节点组合包含重复变量");
                    }
                    if (!_variables.ContainsKey(pair.Var))
                    {
                        throw new ArgumentException($"CPT 父节点未定义: {pair.Var}");
                    }
                    if (!_variables[pair.Var].ValueIndex.ContainsKey(pair.Value))
                    {
                        throw new ArgumentException($"CPT 父节点取值非法: {pair.Var}={pair.Value}");
                    }
                }

                for (int i = 0; i < parents.Count; i++)
                {
                    if (!used.Contains(parents[i]))
                    {
                        throw new ArgumentException($"CPT 缺少父节点 {parents[i]} 的取值组合");
                    }
                }
            }
        }

        private void EnsureTopologicalOrder()
        {
            if (!_topoDirty)
            {
                return;
            }

            var indegree = new Dictionary<string, int>(StringComparer.Ordinal);
            var children = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var kv in _variables)
            {
                indegree[kv.Key] = 0;
                children[kv.Key] = new List<string>();
            }

            foreach (var kv in _nodes)
            {
                var node = kv.Value;
                for (int i = 0; i < node.Parents.Count; i++)
                {
                    string parent = node.Parents[i];
                    indegree[node.Name] += 1;
                    children[parent].Add(node.Name);
                }
            }

            var queue = new Queue<string>();
            foreach (var kv in indegree)
            {
                if (kv.Value == 0)
                {
                    queue.Enqueue(kv.Key);
                }
            }

            var order = new List<string>(_variables.Count);
            while (queue.Count > 0)
            {
                string v = queue.Dequeue();
                order.Add(v);
                var list = children[v];
                for (int i = 0; i < list.Count; i++)
                {
                    string child = list[i];
                    indegree[child] -= 1;
                    if (indegree[child] == 0)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            if (order.Count != _variables.Count)
            {
                throw new InvalidOperationException("贝叶斯网络存在环，无法推断");
            }

            _topoOrder = order;
            _topoDirty = false;
        }

        private Dictionary<string, string> BuildEvidenceMap(EvidenceSet evidence)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (evidence == null)
            {
                return map;
            }

            foreach (var kv in evidence.Values)
            {
                if (!_variables.TryGetValue(kv.Key, out var def))
                {
                    throw new ArgumentException($"证据变量未定义: {kv.Key}");
                }
                if (!def.ValueIndex.ContainsKey(kv.Value))
                {
                    throw new ArgumentException($"证据取值非法: {kv.Key}={kv.Value}");
                }
                map[kv.Key] = kv.Value;
            }

            return map;
        }

        private double EnumerateAll(int index, List<string> order, Dictionary<string, string> assignment)
        {
            if (index >= order.Count)
            {
                return 1d;
            }

            string varName = order[index];
            if (assignment.TryGetValue(varName, out var value))
            {
                double p = GetLocalProbability(varName, value, assignment);
                return p * EnumerateAll(index + 1, order, assignment);
            }

            var def = _variables[varName];
            double sum = 0d;
            for (int i = 0; i < def.Values.Count; i++)
            {
                string val = def.Values[i];
                assignment[varName] = val;
                double p = GetLocalProbability(varName, val, assignment);
                sum += p * EnumerateAll(index + 1, order, assignment);
            }
            assignment.Remove(varName);
            return sum;
        }

        private double GetLocalProbability(string varName, string value, Dictionary<string, string> assignment)
        {
            var node = _nodes[varName];
            if (node.Parents.Count == 0)
            {
                if (node.Prior == null)
                {
                    throw new InvalidOperationException($"变量 {varName} 未设置先验分布");
                }
                return GetValueProbability(node.Prior, value, varName);
            }

            if (node.Conditional == null)
            {
                throw new InvalidOperationException($"变量 {varName} 未设置条件概率表");
            }

            var pairs = new (string Var, string Value)[node.Parents.Count];
            for (int i = 0; i < node.Parents.Count; i++)
            {
                string parent = node.Parents[i];
                if (!assignment.TryGetValue(parent, out var parentValue))
                {
                    throw new InvalidOperationException($"变量 {varName} 缺少父节点 {parent} 的取值");
                }
                pairs[i] = (parent, parentValue);
            }

            var key = new ParentsKey(pairs);
            if (!node.Conditional.TryGetDistribution(key, out var dist))
            {
                throw new InvalidOperationException($"变量 {varName} 缺少父节点组合的 CPT 条目");
            }

            return GetValueProbability(dist, value, varName);
        }

        private float GetValueProbability(Distribution dist, string value, string varName)
        {
            if (!dist.Values.TryGetValue(value, out var p))
            {
                throw new ArgumentException($"变量 {varName} 不存在取值 {value}");
            }
            return p;
        }

        private List<ParentsKey> BuildParentKeys(List<string> parents)
        {
            if (parents == null || parents.Count == 0)
            {
                return new List<ParentsKey> { ParentsKey.Empty };
            }

            var result = new List<ParentsKey>();
            var buffer = new (string Var, string Value)[parents.Count];
            BuildParentKeysRecursive(0, parents, buffer, result);
            return result;
        }

        private void BuildParentKeysRecursive(int index, List<string> parents, (string Var, string Value)[] buffer, List<ParentsKey> result)
        {
            if (index >= parents.Count)
            {
                result.Add(new ParentsKey(buffer));
                return;
            }

            string parent = parents[index];
            var def = _variables[parent];
            for (int i = 0; i < def.Values.Count; i++)
            {
                buffer[index] = (parent, def.Values[i]);
                BuildParentKeysRecursive(index + 1, parents, buffer, result);
            }
        }

        private Distribution BuildDistribution(VariableDef def, float[] counts, float smoothing, Distribution fallback)
        {
            int valueCount = def.Values.Count;
            float sum = 0f;
            var dict = new Dictionary<string, float>(valueCount, StringComparer.Ordinal);
            for (int i = 0; i < valueCount; i++)
            {
                float v = counts[i] + smoothing;
                dict[def.Values[i]] = v;
                sum += v;
            }

            if (sum <= 0f)
            {
                return fallback ?? Distribution.Uniform(def.Values);
            }

            var keys = new List<string>(dict.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                dict[key] = dict[key] / sum;
            }
            return new Distribution(dict, normalize: false);
        }
    }
}
