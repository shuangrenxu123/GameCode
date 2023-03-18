using UnityEngine;

namespace BT
{
    /// <summary>
    /// 选择器会直到有一个返回了成功。
    /// </summary>
    public class BTSelector : BTComposite
    {
        private int _activeChildIndex = -1;
        /// <summary>
        /// 上一个成功的节点序号
        /// </summary>
        private int _previousSuccessChildIndex = -1;

        public int activeChildIndex { get { return _activeChildIndex; } }

        public override void Clear()
        {
            base.Clear();
            switch(clearOpt) 
            {
                case BTClearOpt.Default:
                    if (_activeChildIndex != -1)
                    {
                        children[_activeChildIndex].Clear();
                    }
                    break;

                case BTClearOpt.Selected:
                    foreach (BTNode child in selectedChildrenForClear)
                    {
                        int index = children.IndexOf(child);
                        if (index > _previousSuccessChildIndex)
                        {
                            child.Clear();
                        }
                    }
                    break;

                case BTClearOpt.DefaultAndSelected:
                    if (_activeChildIndex != -1)
                    {
                        BTNode activeChild = children[_activeChildIndex];
                        if (!selectedChildrenForClear.Contains(activeChild))
                        {
                            activeChild.Clear();
                        }
                    }
                    int split = Mathf.Max(_activeChildIndex, _previousSuccessChildIndex);
                    foreach (BTNode child in selectedChildrenForClear)
                    {
                        int index = children.IndexOf(child);
                        if (index > split)
                        {
                            child.Clear();
                        }
                    }
                    break;

                case BTClearOpt.All:
                    split = Mathf.Max(_activeChildIndex - 1, _previousSuccessChildIndex);
                    foreach (BTNode child in children)
                    {
                        int index = children.IndexOf(child);
                        if (index > split)
                        {
                            child.Clear();
                        }
                    }
                    break;
            }
        }
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
                    isRunning= true;
                    return BTResult.Running;
                }
            }
            _activeChildIndex = -1;
            _previousSuccessChildIndex = -1;
            isRunning= false;
            return BTResult.Failed;
        }
    }
}
