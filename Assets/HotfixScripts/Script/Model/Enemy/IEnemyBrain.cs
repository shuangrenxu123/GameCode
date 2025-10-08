using CharacterController;
using UnityEngine;

namespace Enemy
{
    public interface IEnemyBrain
    {
        /// <summary>
        /// 用于控制输入
        /// </summary>
        CharacterBrain characterBrain { get; }

        CharacterActions characterActions { get; set; }

        /// <summary>
        /// 初始化大脑，注入身体引用
        /// </summary>
        /// <param name="body">敌人的身体组件</param>
        void Initialize(Enemy body);

        /// <summary>
        /// 思考过程（每帧调用）
        /// </summary>
        void Think();

        /// <summary>
        /// 关闭大脑
        /// </summary>
        void Shutdown();
    }
}