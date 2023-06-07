
namespace Fight {
    /// <summary>
    /// 一次行为
    /// </summary>
    public abstract class CombatAction
    {
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
        protected abstract void PreProcess(CombatEntity c,CombatEntity t);
        /// <summary>
        /// 应用
        /// </summary>
        public abstract void Apply(int baseValue);
        /// <summary>
        /// 后续
        /// </summary>
        protected abstract void PostProcess(CombatEntity c,CombatEntity t);
    }
}
