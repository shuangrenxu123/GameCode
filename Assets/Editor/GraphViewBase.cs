using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class GraphViewBase : GraphView
{

    public readonly Vector2 NodeSize = new(100, 150);
    public abstract NodeBase CreatNode(string title, Vector2 position);
    public abstract void AddChoicePort(NodeBase node, string name, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Multi);


}
