namespace BT
{
    /// <summary>
    /// ȡ�����νڵ�
    /// </summary>
    public class BTInverted : BTDecorator
    {
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
