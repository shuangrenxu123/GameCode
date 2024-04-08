namespace BT
{
    /// <summary>
    /// 顺序节点，从左往右执行子节点，知道所有的都成功或者有一个返回失败
    /// </summary>
    public class BTSequence : BTComposite
    {
        /// <summary>
        /// 激活节点的序号
        /// </summary>
        private int _activeChildIndex = -1;

        /// <summary>
        /// 执行一次，会首先执行第一个
        /// </summary>
        /// <returns></returns>
        public override BTResult Tick()
        {
            if (_activeChildIndex == -1)
            {
                _activeChildIndex = 0;
            }
            BTResult result = BTResult.Success;
            while (_activeChildIndex != children.Count)
            {
                var child = children[_activeChildIndex];
                var childResult = child.Tick();
                if (childResult == BTResult.Failed)
                {
                    child.Clear();
                    result = BTResult.Failed;
                    break;
                }
                if (childResult == BTResult.Success)
                {
                    child.Clear();
                    _activeChildIndex += 1;
                    continue;
                }
                else
                {
                    isRunning = true;
                    return BTResult.Running;
                }
            }
            _activeChildIndex = -1;
            isRunning = false;
            return result;
        }
        public override void Clear()
        {
            base.Clear();
            if (_activeChildIndex > -1)
            {
                for (int i = _activeChildIndex; i < children.Count; i++)
                {
                    if (i < 0) continue;
                    children[i].Clear();
                }
            }
            _activeChildIndex = -1;
        }
    }
}
