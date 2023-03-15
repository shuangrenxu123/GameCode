
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
        public CombatEntity Target { get; set; }
        /// <summary>
        /// 前置
        /// </summary>
        public abstract void PreProcess();
        /// <summary>
        /// 应用
        /// </summary>
        public abstract void Apply();
        /// <summary>
        /// 后续
        /// </summary>
        public abstract void PostProcess();
    }
}
