using System.Collections.Generic;

namespace BT
{
    /// <summary>
    /// 
    ///并行节点。BTParallel 
    ///所有的节点都会并行运行，当某一个返回失败的时候就会返回失败，所有都返回成功才会返回成功
    ///
    /// 当为And时候，需要等待所有的子节点都运行完毕，
    /// 当为Or时 只要有一个节点返回成功或失败他就会返回
    /// </summary>
    public class BTParallel : BTComposite
    {
        private ParallelType ParallelType;
        private List<BTResult> _results;
        private int endingResultCount;
        public BTParallel(ParallelType type)
        {
            ParallelType = type;
            _results = new List<BTResult>();
        }
        public override BTResult Tick()
        {
            switch (ParallelType)
            {
                case ParallelType.And:
                    endingResultCount = 0;
                    TickAllChildren();
                    if (endingResultCount == children.Count)
                    {
                        foreach (var result in _results)
                        {
                            if (result == BTResult.Failed)
                                return BTResult.Failed;
                        }
                        return BTResult.Success;
                    }
                    break;
                case ParallelType.Or:
                    TickAllChildren();
                    foreach (BTResult result in _results)
                    {
                        if (result != BTResult.Running)
                        {
                            var t = result;
                            ResetResults();
                            return result;
                        }
                    }
                    break;
                case ParallelType.Wait:
                    endingResultCount = 0;
                    ResetResults();
                    TickAllChildren();
                    if (endingResultCount == children.Count)
                    {
                        foreach (var result in _results)
                        {
                            if (result == BTResult.Failed)
                                return BTResult.Failed;
                        }
                        return BTResult.Success;
                    }
                    break;
            }
            return BTResult.Running;
        }

        private void TickAllChildren()
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (_results[i] == BTResult.Running)
                {
                    _results[i] = children[i].Tick();
                }
                if (_results[i] != BTResult.Running)
                {
                    endingResultCount++;
                }
            }
        }
        public override BTComposite AddChild(BTNode node)
        {
            base.AddChild(node);
            _results.Add(BTResult.Running);
            return this;
        }
        private void ResetResults()
        {
            for (int i = 0; i < _results.Count; i++)
            {
                _results[i] = BTResult.Running;
            }
        }
        public override void Clear()
        {
            base.Clear();
            foreach (BTNode child in children)
            {
                child.Clear();
            }
        }
    }
}
