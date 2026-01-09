using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BT.Editor.RuntimeJson
{
    static class BTGraphRootInitializer
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        static readonly Type GraphModelImpType =
            Type.GetType("Unity.GraphToolkit.Editor.Implementation.GraphModelImp, Unity.GraphToolkit.Editor", throwOnError: false);

        public static void EnsureRootNodeExists(BTTreeGraph graph)
        {
            if (graph == null)
                return;

            try
            {
                if (graph.GetNodes().OfType<BTEditorRootNode>().Any())
                    return;

                var graphModel = GetGraphModel(graph);
                if (graphModel == null)
                    return;

                var node = new BTEditorRootNode();
                InvokeCreateNodeModel(graphModel, node, Vector2.zero);
                node.DefineNode();
            }
            catch
            {
                // Keep silent: this runs during editor lifecycle and should not block.
            }
        }

        static object GetGraphModel(BTTreeGraph graph)
        {
            var implField = typeof(Unity.GraphToolkit.Editor.Graph).GetField("m_Implementation", AnyInstance);
            return implField?.GetValue(graph);
        }

        static void InvokeCreateNodeModel(object graphModel, Unity.GraphToolkit.Editor.Node node, Vector2 position)
        {
            if (GraphModelImpType == null || graphModel == null || !GraphModelImpType.IsInstanceOfType(graphModel))
                return;

            var method = GraphModelImpType.GetMethod("CreateNodeModel", AnyInstance, null,
                new[] { typeof(Unity.GraphToolkit.Editor.Node), typeof(Vector2) }, null);
            method?.Invoke(graphModel, new object[] { node, position });
        }
    }
}

