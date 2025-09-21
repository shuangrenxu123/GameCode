namespace BT
{
    /// <summary>
    /// ���Խڵ�
    /// </summary>
    public class BTRepeat<TKey, TValue> : BTDecorator<TKey, TValue>
    {
        public BTRepeat(BTNode<TKey, TValue> child) : base(child)
        {

        }
        public override BTResult Tick()
        {
            var result = child.Tick();
            if (result == BTResult.Success)
            {
                return BTResult.Success;

            }
            if (result == BTResult.Failed)
            {
                child.Clear();
            }
            return BTResult.Running;
        }
    }
}