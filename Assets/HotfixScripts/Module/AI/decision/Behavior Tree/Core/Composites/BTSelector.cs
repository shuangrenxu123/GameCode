namespace BT
{
    /// <summary>
    /// 选择器会直到有一个返回了成功。
    /// </summary>
    public class BTSelector<TKey, TValue> : BTComposite<TKey, TValue>
    {
        private int _activeChildIndex = -1;
        /// <summary>
        /// 上一个成功的节点序号
        /// </summary>
        private int _previousSuccessChildIndex = -1;

        public int activeChildIndex { get { return _activeChildIndex; } }

        public override BTResult Tick()
        {
            if (children.Count == 0)
                return BTResult.Failed;
            while (activeChildIndex != children.Count)
            {
                if (_activeChildIndex == -1)
                {
                    _activeChildIndex = 0;
                }
                var child = children[activeChildIndex];
                var result = child.Tick();
                if (result == BTResult.Success)
                {
                    child.Clear();
                    _previousSuccessChildIndex = activeChildIndex;
                    _activeChildIndex = -1;
                    isRunning = false;
                    return BTResult.Success;
                }
                if (result == BTResult.Failed)
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
            _previousSuccessChildIndex = -1;
            isRunning = false;
            return BTResult.Failed;
        }
    }
}
