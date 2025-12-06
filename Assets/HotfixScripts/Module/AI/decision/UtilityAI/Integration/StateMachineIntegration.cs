using System;
using System.Collections.Generic;
using AIBlackboard;
using HFSM;

namespace UtilityAI.Integration
{
    /// <summary>
    /// 效用AI驱动的状态 - 在HFSM状态内部运行效用AI
    /// </summary>
    /// <typeparam name="T">状态枚举类型</typeparam>
    public abstract class UtilityAIState<T> : StateBase<T> where T : Enum
    {
        protected UtilityBrain brain;
        protected Dictionary<string, T> optionToStateMap = new Dictionary<string, T>();

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

        /// <summary>
        /// 映射选项名称到目标状态
        /// </summary>
        protected void MapOptionToState(string optionName, T state)
        {
            optionToStateMap[optionName] = state;
        }

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input);
            brain.Start();
        }

        public override void Update()
        {
            base.Update();
            brain.Update();

            CheckDecision();
        }

        private void CheckDecision()
        {
            var decision = database.GetValue<EnemyAIDatabaseKey, UtilityDecision>(EnemyAIDatabaseKey.UtilityDecision);
            if (decision.IsValid && optionToStateMap.TryGetValue(decision.OptionName, out T targetState))
            {
                // 如果目标状态不是当前状态（虽然我们在UtilityAIState内部，但这里指的是外部状态机的状态切换）
                // 注意：这里假设UtilityAIState是状态机的一个状态，它负责根据决策切换到其他状态
                // 如果其他状态也是UtilityAIState，那么可能会形成循环，需要小心配置

                // 这里我们直接请求状态机切换
                // 只有当目标状态与当前状态不同时才切换？
                // 但UtilityAIState本身就是当前状态。所以如果决策指向其他状态，就切换。
                // 如果决策指向自己（或者没有映射），则保持。

                // if (!stateMachine.CurrentStateType.Equals(targetState))
                // {
                //     stateMachine.ChangeState(targetState);
                // }
            }
        }

        public override void Exit()
        {
            brain.Stop();
            base.Exit();
        }
    }
}
