using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BT.RuntimeSerialization
{
    public static class BTTreeRuntimeBuilder
    {
        public static BTNode BuildFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON is empty.", nameof(json));

            var dto = JsonUtility.FromJson<BTTreeRuntimeJson>(json);
            if (dto == null)
                throw new ArgumentException("Failed to parse BT tree JSON.", nameof(json));

            return Build(dto);
        }

        public static BTNode Build(BTTreeRuntimeJson dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.nodes == null || dto.nodes.Count == 0)
                return null;

            var nodesById = dto.nodes
                .Where(n => n != null && !string.IsNullOrEmpty(n.id))
                .ToDictionary(n => n.id, n => n);

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
    }
}

