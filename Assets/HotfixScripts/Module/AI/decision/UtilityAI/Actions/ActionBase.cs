using AIBlackboard;

namespace UtilityAI
{
    /// <summary>
    /// 动作基类 - 提供常用功能的默认实现
    /// </summary>
    public abstract class ActionBase : IAction
    {
        public abstract string Name { get; }
        public ActionState State { get; protected set; } = ActionState.Ready;

        public virtual void Enter(Blackboard blackboard)
        {
            State = ActionState.Running;
        }

        public virtual ActionState Execute(Blackboard blackboard)
        {
            return State;
        }

        public virtual void Exit(Blackboard blackboard)
        {
            State = ActionState.Ready;
        }

        public virtual void Abort(Blackboard blackboard)
        {
            State = ActionState.Ready;
        }

        /// <summary>
        /// 标记动作成功完成
        /// </summary>
        protected void Success()
        {
            State = ActionState.Success;
        }

        /// <summary>
        /// 标记动作失败
        /// </summary>
        protected void Fail()
        {
            State = ActionState.Failed;
        }
    }

    /// <summary>
    /// 即时动作 - 立即完成的动作
    /// </summary>
    public abstract class InstantAction : ActionBase
    {
        public sealed override ActionState Execute(Blackboard blackboard)
        {
            bool success = DoAction(blackboard);
            State = success ? ActionState.Success : ActionState.Failed;
            return State;
        }

        /// <summary>
        /// 执行即时动作
        /// </summary>
        /// <returns>是否成功</returns>
        protected abstract bool DoAction(Blackboard blackboard);
    }

    /// <summary>
    /// 持续动作 - 需要多帧执行的动作
    /// </summary>
    public abstract class ContinuousAction : ActionBase
    {
        public sealed override ActionState Execute(Blackboard blackboard)
        {
            State = OnUpdate(blackboard);
            return State;
        }

        /// <summary>
        /// 每帧更新，返回当前状态
        /// </summary>
        protected abstract ActionState OnUpdate(Blackboard blackboard);
    }

    /// <summary>
    /// 定时动作 - 执行指定时间后自动完成
    /// </summary>
    public class TimedAction : ActionBase
    {
        public override string Name { get; }

        private readonly float duration;
        private float startTime;
        private readonly System.Action<Blackboard> onStart;
        private readonly System.Action<Blackboard, float> onUpdate;
        private readonly System.Action<Blackboard> onComplete;

        public TimedAction(
            string name,
            float duration,
            System.Action<Blackboard> onStart = null,
            System.Action<Blackboard, float> onUpdate = null,
            System.Action<Blackboard> onComplete = null)
        {
            Name = name;
            this.duration = duration;
            this.onStart = onStart;
            this.onUpdate = onUpdate;
            this.onComplete = onComplete;
        }

        public override void Enter(Blackboard blackboard)
        {
            base.Enter(blackboard);
            startTime = UnityEngine.Time.time;
            onStart?.Invoke(blackboard);
        }

        public override ActionState Execute(Blackboard blackboard)
        {
            float elapsed = UnityEngine.Time.time - startTime;
            float progress = UnityEngine.Mathf.Clamp01(elapsed / duration);

            onUpdate?.Invoke(blackboard, progress);

            if (elapsed >= duration)
            {
                onComplete?.Invoke(blackboard);
                State = ActionState.Success;
            }

            return State;
        }
    }

    /// <summary>
    /// Lambda动作 - 使用委托快速创建动作
    /// </summary>
    public class LambdaAction : ActionBase
    {
        public override string Name { get; }

        private readonly System.Action<Blackboard> enterAction;
        private readonly System.Func<Blackboard, ActionState> executeAction;
        private readonly System.Action<Blackboard> exitAction;

        public LambdaAction(
            string name,
            System.Action<Blackboard> enter = null,
            System.Func<Blackboard, ActionState> execute = null,
            System.Action<Blackboard> exit = null)
        {
            Name = name;
            enterAction = enter;
            executeAction = execute;
            exitAction = exit;
        }

        public override void Enter(Blackboard blackboard)
        {
            base.Enter(blackboard);
            enterAction?.Invoke(blackboard);
        }

        public override ActionState Execute(Blackboard blackboard)
        {
            if (executeAction != null)
            {
                State = executeAction(blackboard);
            }
            else
            {
                State = ActionState.Success;
            }
            return State;
        }

        public override void Exit(Blackboard blackboard)
        {
            exitAction?.Invoke(blackboard);
            base.Exit(blackboard);
        }
    }

    /// <summary>
    /// 空动作 - 什么都不做
    /// </summary>
    public class IdleAction : InstantAction
    {
        public override string Name => "Idle";

        protected override bool DoAction(Blackboard blackboard) => true;
    }
}
