using AIBlackboard;

namespace UtilityAI
{
    /// <summary>
    /// 效用AI动作的执行状态
    /// </summary>
    public enum ActionState
    {
        /// <summary>
        /// 准备执行
        /// </summary>
        Ready,
        /// <summary>
        /// 正在执行
        /// </summary>
        Running,
        /// <summary>
        /// 执行成功
        /// </summary>
        Success,
        /// <summary>
        /// 执行失败
        /// </summary>
        Failed
    }

    /// <summary>
    /// 效用AI动作接口 - 定义AI可以执行的行为
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 当前执行状态
        /// </summary>
        ActionState State { get; }

        /// <summary>
        /// 开始执行动作
        /// </summary>
        void Enter(Blackboard blackboard);

        /// <summary>
        /// 每帧更新，返回当前状态
        /// </summary>
        ActionState Execute(Blackboard blackboard);

        /// <summary>
        /// 结束动作（无论成功或失败）
        /// </summary>
        void Exit(Blackboard blackboard);

        /// <summary>
        /// 动作被中断时调用
        /// </summary>
        void Abort(Blackboard blackboard);
    }
}
