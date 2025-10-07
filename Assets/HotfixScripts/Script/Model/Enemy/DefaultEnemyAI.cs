using BT;
using UnityEngine;

namespace Enemy.AI
{
    public class DefaultEnemyAI : MonoBehaviour, IEnemyBrain
    {
        private Enemy body;
        private EnemyBT<EnemyAIDatabaseKey, object> behaviorTree;

        /// <summary>
        /// 初始化大脑，注入身体引用
        /// </summary>
        /// <param name="enemyBody">敌人的身体组件</param>
        public void Initialize(Enemy enemyBody)
        {
            body = enemyBody;

            // 初始化行为树数据库
            var database = new DataBase<EnemyAIDatabaseKey, object>();
            database.SetData(EnemyAIDatabaseKey.CharacterActor, body.characterActor);
            database.SetData(EnemyAIDatabaseKey.CombatEntity, body.combatEntity);
            database.SetData(EnemyAIDatabaseKey.Transform, body.transform);
            database.SetData(EnemyAIDatabaseKey.EnemyBody, body);

            // 创建行为树大脑
            behaviorTree = new EnemyBT<EnemyAIDatabaseKey, object>();
            behaviorTree.Init(database);
        }


        public void PlayAnimation(string animationName)
        {
            if (body != null && body.animancerHelper != null && body.animatorConfig != null)
            {
                // 尝试从剪辑动画字典中获取动画
                if (body.animatorConfig.clipAnimators.TryGetValue(animationName, out var clip))
                {
                    body.animancerHelper.Play(clip);
                }
                // 尝试从线性混合动画字典中获取动画
                else if (body.animatorConfig.linearMixerAnimators.TryGetValue(animationName, out var mixer))
                {
                    body.animancerHelper.Play(mixer);
                }
                else
                {
                    Debug.LogWarning($"未找到动画: {animationName}");
                }
            }
        }


        public void PlayMovementAnimation()
        {
            PlayAnimation("NormalMove");
        }

        /// <summary>
        /// 思考过程（每帧调用）
        /// </summary>
        public void Think()
        {
            // 更新动画（基于移动状态）
            if (body != null && body.characterActor != null)
            {
                PlayMovementAnimation();
            }

            // 执行行为树逻辑
            // behaviorTree.Update();
        }

        /// <summary>
        /// 关闭大脑
        /// </summary>
        public void Shutdown()
        {
            // 清理资源
            behaviorTree = null;
        }
    }
}