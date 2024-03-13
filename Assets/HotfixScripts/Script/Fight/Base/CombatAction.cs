
using System;
using System.Collections.Generic;

namespace Fight
{
    //todo 思考关于函数内处理的优先级问题
    /// <summary>
    /// 对某个行为的封装
    /// </summary>
    public abstract class CombatAction
    {
        private Action<CombatAction> PreCreatorAction;
        private Action<CombatAction> PostCreatorAction;
        private List<Action<CombatAction>> PreTargetActions = new ();
        private List<Action<CombatAction>> PostTargetActions = new ();
        /// <summary>
        /// 发起者
        /// </summary>
        public CombatEntity Creator { get; set; }
        /// <summary>
        /// 对象
        /// </summary>
        public CombatEntity[] Target { get; set; }
        /// <summary>
        /// 前置
        /// </summary>
        protected abstract void PreProcess(CombatEntity c, CombatEntity t);
        /// <summary>
        /// 应用
        /// </summary>
        public abstract void Apply(int baseValue);
        /// <summary>
        /// 后续
        /// </summary>
        protected abstract void PostProcess(CombatEntity c, CombatEntity t);

        public void AddPreCreatorAction(Action<CombatAction> action)
        {
            PreCreatorAction = action;
        }
        public void AddPostCreatorAction(Action<CombatAction> action)
        {
            PostCreatorAction = action;
        }
        public void AddPreTargetAction(Action<CombatAction> action)
        {
            PreTargetActions.Add(action);
        }
        public void AddPostTargetAction(Action<CombatAction> action)
        {
            PostTargetActions.Add(action);
        }
    }
}
