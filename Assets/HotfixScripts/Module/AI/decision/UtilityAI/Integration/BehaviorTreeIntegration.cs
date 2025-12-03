using AIBlackboard;
using BT;

namespace UtilityAI.Integration
{
    /// <summary>
    /// 行为树动作包装器 - 将行为树作为效用AI的动作执行
    /// 实现效用AI与行为树的无缝集成
    /// </summary>
    public class BehaviorTreeAction : IAction
    {
        public string Name { get; }
        public ActionState State { get; private set; } = ActionState.Ready;

        private readonly BTTree behaviorTree;
        private BTResult lastResult = BTResult.Running;

        /// <summary>
        /// 创建行为树动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="tree">要执行的行为树</param>
        public BehaviorTreeAction(string name, BTTree tree)
        {
            Name = name;
            behaviorTree = tree;
        }

        public void Enter(Blackboard blackboard)
        {
            State = ActionState.Running;
            lastResult = BTResult.Running;

            // 共享黑板数据
            if (behaviorTree.database != blackboard)
            {
                behaviorTree.Init(blackboard);
            }
        }

        public ActionState Execute(Blackboard blackboard)
        {
            if (behaviorTree == null)
            {
                State = ActionState.Failed;
                return State;
            }

            behaviorTree.Update();

            // 根据行为树根节点的状态判断动作是否完成
            // 注意：需要行为树内部跟踪执行结果
            // 这里简化处理，假设树会在黑板中设置结果
            if (blackboard.TryGetValue<BTResult>("_btResult", out var result))
            {
                lastResult = result;
            }

            State = lastResult switch
            {
                BTResult.Success => ActionState.Success,
                BTResult.Failed => ActionState.Failed,
                _ => ActionState.Running
            };

            return State;
        }

        public void Exit(Blackboard blackboard)
        {
            State = ActionState.Ready;
        }

        public void Abort(Blackboard blackboard)
        {
            // 清理行为树状态
            State = ActionState.Ready;
        }
    }

    /// <summary>
    /// 行为树节点动作包装器 - 将单个BTNode作为效用AI动作
    /// </summary>
    public class BTNodeAction : IAction
    {
        public string Name { get; }
        public ActionState State { get; private set; } = ActionState.Ready;

        private readonly BTNode node;
        private bool isActivated;

        public BTNodeAction(string name, BTNode node)
        {
            Name = name;
            this.node = node;
        }

        public void Enter(Blackboard blackboard)
        {
            State = ActionState.Running;

            if (!isActivated)
            {
                node.Activate(blackboard);
                isActivated = true;
            }
        }

        public ActionState Execute(Blackboard blackboard)
        {
            var result = node.Tick();

            State = result switch
            {
                BTResult.Success => ActionState.Success,
                BTResult.Failed => ActionState.Failed,
                _ => ActionState.Running
            };

            return State;
        }

        public void Exit(Blackboard blackboard)
        {
            node.Clear();
            State = ActionState.Ready;
        }

        public void Abort(Blackboard blackboard)
        {
            node.Clear();
            State = ActionState.Ready;
        }
    }
}
