using System;
using AIBlackboard;
using HFSM;

namespace UtilityAI.Integration
{
    /// <summary>
    /// 状态机动作包装器 - 将状态机状态切换作为效用AI的动作
    /// 实现效用AI与HFSM的无缝集成
    /// </summary>
    /// <typeparam name="T">状态枚举类型</typeparam>
    public class StateMachineAction<T> : IAction where T : Enum
    {
        public string Name { get; }
        public ActionState State { get; private set; } = ActionState.Ready;

        private readonly StateMachine<T> stateMachine;
        private readonly T targetState;
        private readonly Func<Blackboard, bool> completionCondition;
        private readonly bool waitForStateChange;

        /// <summary>
        /// 创建状态机动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="stateMachine">目标状态机</param>
        /// <param name="targetState">要切换到的状态</param>
        /// <param name="completionCondition">动作完成条件（可选）</param>
        /// <param name="waitForStateChange">是否等待状态切换完成</param>
        public StateMachineAction(
            string name,
            StateMachine<T> stateMachine,
            T targetState,
            Func<Blackboard, bool> completionCondition = null,
            bool waitForStateChange = false)
        {
            Name = name;
            this.stateMachine = stateMachine;
            this.targetState = targetState;
            this.completionCondition = completionCondition;
            this.waitForStateChange = waitForStateChange;
        }

        public void Enter(Blackboard blackboard)
        {
            State = ActionState.Running;

            // 触发状态切换
            if (!stateMachine.CurrentStateType.Equals(targetState))
            {
                stateMachine.ChangeState(targetState);
            }
        }

        public ActionState Execute(Blackboard blackboard)
        {
            // 如果有完成条件，检查条件
            if (completionCondition != null)
            {
                if (completionCondition(blackboard))
                {
                    State = ActionState.Success;
                }
                return State;
            }

            // 如果需要等待状态切换
            if (waitForStateChange)
            {
                if (stateMachine.CurrentStateType.Equals(targetState))
                {
                    State = ActionState.Success;
                }
                return State;
            }

            // 默认立即完成
            State = ActionState.Success;
            return State;
        }

        public void Exit(Blackboard blackboard)
        {
            State = ActionState.Ready;
        }

        public void Abort(Blackboard blackboard)
        {
            State = ActionState.Ready;
        }
    }

    /// <summary>
    /// 效用AI驱动的状态 - 在HFSM状态内部运行效用AI
    /// </summary>
    /// <typeparam name="T">状态枚举类型</typeparam>
    public abstract class UtilityAIState<T> : StateBase<T> where T : Enum
    {
        protected UtilityBrain brain;

        public override void Init()
        {
            base.Init();
            brain = new UtilityBrain(database);
            SetupOptions();
        }

        /// <summary>
        /// 配置效用AI选项
        /// </summary>
        protected abstract void SetupOptions();

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input);
            brain.Start();
        }

        public override void Update()
        {
            base.Update();
            brain.Update();
        }

        public override void Exit()
        {
            brain.Stop();
            base.Exit();
        }
    }
}
