
using System;
using System.Collections.Generic;
using ObjectPool;

namespace Fight
{
    //todo 思考关于函数内处理的优先级问题
    /// <summary>
    /// 对某个行为的封装
    /// </summary>
    public abstract class CombatAction : IReferenceObject
    {
        protected Action<CombatAction> PreCreatorAction;
        protected Action<CombatAction> PostCreatorAction;
        protected List<Action<CombatAction>> PreTargetActions = new();
        protected List<Action<CombatAction>> PostTargetActions = new();

        public virtual void Setup(CombatEntity creator, List<CombatEntity> target)
        {
            Creator = creator;
            Target = target;
        }


        /// <summary>
        /// 发起者
        /// </summary>
        public CombatEntity Creator { get; set; }
        /// <summary>
        /// 对象
        /// </summary>
        public List<CombatEntity> Target { get; set; }
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

        public CombatAction AddPreCreatorAction(Action<CombatAction> action)
        {
            PreCreatorAction = action;
            return this;
        }
        public CombatAction AddPostCreatorAction(Action<CombatAction> action)
        {
            PostCreatorAction = action;
            return this;
        }
        public CombatAction AddPreTargetAction(Action<CombatAction> action)
        {
            PreTargetActions.Add(action);
            return this;
        }
        public CombatAction AddPostTargetAction(Action<CombatAction> action)
        {
            PostTargetActions.Add(action);
            return this;
        }

        public virtual void OnInit()
        {
        }
        public virtual void OnRelease()
        {
            PreCreatorAction = null;
            PostCreatorAction = null;
            PreTargetActions.Clear();
            PostTargetActions.Clear();
        }

        protected void Release()
        {
            ReferenceManager.Instance.Release(this);
        }
    }
}
