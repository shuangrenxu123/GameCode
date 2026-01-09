namespace BT
{
    /// <summary>
    /// ȡ�����νڵ�
    /// </summary>
    [BT.EditorIntegration.BTEditorNode("Decorator/Inverted", BT.EditorIntegration.BTEditorNodeKind.Decorator)]
    public class BTInverted : BTDecorator
    {
        [BT.EditorIntegration.BTEditorConstructor]
        public BTInverted(BTNode child) : base(child)
        {

        }
        public override BTResult Tick()
        {
            var result = child.Tick();
            if (result == BTResult.Success)
            {
                return BTResult.Failed;
            }
            if (result == BTResult.Failed)
            {
                return BTResult.Success;
            }
            return BTResult.Running;
        }
    }
}
