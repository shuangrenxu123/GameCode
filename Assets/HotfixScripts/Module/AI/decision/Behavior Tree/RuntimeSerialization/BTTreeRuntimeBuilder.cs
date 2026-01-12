using System;
using System.Collections.Generic;
using System.Linq;
using AIBlackboard;

namespace BT.RuntimeSerialization
{
    public static class BTTreeRuntimeBuilder
    {
        static BTTreeRuntimeJson Parse(string json)
        {
            try
            {
                var dto = UnityEngine.JsonUtility.FromJson<BTTreeRuntimeJson>(json);
                if (dto != null)
                    return dto;
            }
            catch
            {
                // ignore, fallback
            }

            try
            {
                return Utf8Json.JsonSerializer.Deserialize<BTTreeRuntimeJson>(json);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to parse BT tree JSON: {e.Message}", nameof(json), e);
            }
        }

        public static BTNode BuildFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON is empty.", nameof(json));

            var dto = Parse(json);
            if (dto == null)
                throw new ArgumentException("Failed to parse BT tree JSON.", nameof(json));

            return Build(dto);
        }

        public static BTNode BuildFromJson(string json, out Blackboard blackboard)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON is empty.", nameof(json));

            var dto = Parse(json);
            if (dto == null)
                throw new ArgumentException("Failed to parse BT tree JSON.", nameof(json));

            blackboard = BuildBlackboard(dto);
            return Build(dto);
        }

        public static BTNode Build(BTTreeRuntimeJson dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.nodes == null || dto.nodes.Count == 0)
                return null;

            var nodesById = new Dictionary<string, BTNodeRuntimeJson>(StringComparer.Ordinal);
            for (var i = 0; i < dto.nodes.Count; i++)
            {
                var node = dto.nodes[i];
                if (node == null || string.IsNullOrEmpty(node.id))
                    continue;

                if (nodesById.ContainsKey(node.id))
                    throw new InvalidOperationException($"BT tree JSON contains duplicated node id '{node.id}'. 请重新导出JSON或修复编辑器图中重复的节点ID。");

                nodesById.Add(node.id, node);
            }

            if (string.IsNullOrEmpty(dto.rootId))
            {
                var rootCandidate = FindRootCandidate(nodesById.Values);
                dto.rootId = rootCandidate?.id;
            }

            if (string.IsNullOrEmpty(dto.rootId) || !nodesById.ContainsKey(dto.rootId))
                throw new InvalidOperationException("BT tree JSON has no valid rootId.");

            var built = new Dictionary<string, BTNode>(StringComparer.Ordinal);
            var visiting = new HashSet<string>(StringComparer.Ordinal);

            return BuildNode(dto.rootId);

            BTNode BuildNode(string id)
            {
                if (built.TryGetValue(id, out var cached))
                    return cached;

                if (!nodesById.TryGetValue(id, out var nodeDto))
                    return null;

                if (!visiting.Add(id))
                    throw new InvalidOperationException($"Cycle detected at node '{id}'.");

                var kind = BTGeneratedNodeFactory.GetKind(nodeDto.typeId);

                BTNode result;
                switch (kind)
                {
                    case BTGeneratedNodeFactory.NodeKind.Decorator:
                    {
                        var childId = nodeDto.children != null && nodeDto.children.Count > 0 ? nodeDto.children[0] : null;
                        var child = string.IsNullOrEmpty(childId) ? null : BuildNode(childId);
                        result = BTGeneratedNodeFactory.CreateDecorator(nodeDto.typeId, nodeDto.args, child);
                        break;
                    }

                    case BTGeneratedNodeFactory.NodeKind.Composite:
                    {
                        result = BTGeneratedNodeFactory.CreateComposite(nodeDto.typeId, nodeDto.args);
                        if (result is BTComposite composite && nodeDto.children != null)
                        {
                            foreach (var childId in nodeDto.children)
                            {
                                var child = string.IsNullOrEmpty(childId) ? null : BuildNode(childId);
                                if (child != null)
                                    composite.AddChild(child);
                            }
                        }
                        break;
                    }

                    default:
                    {
                        result = BTGeneratedNodeFactory.CreateLeaf(nodeDto.typeId, nodeDto.args);
                        break;
                    }
                }

                visiting.Remove(id);
                built[id] = result;
                return result;
            }
        }

        static BTNodeRuntimeJson FindRootCandidate(IEnumerable<BTNodeRuntimeJson> nodes)
        {
            var list = nodes?.Where(n => n != null && !string.IsNullOrEmpty(n.id)).ToList() ?? new List<BTNodeRuntimeJson>();
            if (list.Count == 0)
                return null;

            var hasParent = new HashSet<string>(StringComparer.Ordinal);
            foreach (var n in list)
            {
                if (n.children == null)
                    continue;
                foreach (var childId in n.children)
                {
                    if (!string.IsNullOrEmpty(childId))
                        hasParent.Add(childId);
                }
            }

            return list.FirstOrDefault(n => !hasParent.Contains(n.id)) ?? list[0];
        }

        public static Blackboard BuildBlackboard(BTTreeRuntimeJson dto)
        {
            var bb = new Blackboard();

            if (dto?.blackboard == null)
                return bb;

            for (var i = 0; i < dto.blackboard.Count; i++)
            {
                var entry = dto.blackboard[i];
                if (entry == null || string.IsNullOrEmpty(entry.key))
                    continue;

                switch (entry.type)
                {
                    case BTBlackboardValueType.Int:
                        if (int.TryParse(entry.value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var iVal))
                            bb.SetValue<string, int>(entry.key, iVal);
                        else
                            bb.SetValue<string, int>(entry.key, 0);
                        break;

                    case BTBlackboardValueType.Float:
                        if (float.TryParse(entry.value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fVal))
                            bb.SetValue<string, float>(entry.key, fVal);
                        else
                            bb.SetValue<string, float>(entry.key, 0f);
                        break;

                    case BTBlackboardValueType.Bool:
                        if (bool.TryParse(entry.value, out var bVal))
                            bb.SetValue<string, bool>(entry.key, bVal);
                        else
                            bb.SetValue<string, bool>(entry.key, false);
                        break;

                    case BTBlackboardValueType.String:
                    default:
                        bb.SetValue<string, string>(entry.key, entry.value ?? string.Empty);
                        break;
                }
            }

            return bb;
        }
    }
}
