using AIBlackboard;
using BT;
using CharacterController;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    /// <summary>
    /// 减速巡逻节点：以配置的速度倍率（默认0.5）在初始位置附近巡逻
    /// </summary>
    public class BTSlowPatrolNode : BTAction
    {
        private Enemy.Enemy enemy;
        private CharacterActions characterActions;
        private float arrivalThreshold = 0.5f;
        private float speedMultiplier = 0.5f;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            enemy = database.GetValue<EnemyAIDatabaseKey, Enemy.Enemy>(EnemyAIDatabaseKey.EnemyBody);
            characterActions = database.GetValue<string, CharacterActions>("characterActions");
            speedMultiplier = database.GetValue(EnemyAIDatabaseKey.PatrolSpeedMultiplier, 0.5f);
        }

        protected override BTResult Execute()
        {
            if (enemy == null || characterActions == null)
            {
                Debug.LogError("[减速巡逻] 敌人或角色动作组件为空！");
                return BTResult.Failed;
            }

            var destination = GetOrCreateDestination();
            Vector3 direction = destination - enemy.characterActor.transform.position;

            Debug.Log($"[减速巡逻] 当前位置: {enemy.characterActor.transform.position}, 目标位置: {destination}, 距离: {direction.magnitude:F2}");

            if (direction.magnitude <= arrivalThreshold)
            {
                GenerateNewDestination();
                StopMovement();
                Debug.Log("[减速巡逻] 到达目标点，生成新目标");
                return BTResult.Success;
            }

            // 确保攻击关闭
            characterActions.attack.value = false;

            ApplySlowInput(direction);
            return BTResult.Running;
        }

        private Vector3 GetOrCreateDestination()
        {
            if (!database.TryGetValue(EnemyAIDatabaseKey.PatrolDestination, out Vector3 destination))
            {
                destination = GenerateNewDestination();
            }
            return destination;
        }

        private Vector3 GenerateNewDestination()
        {
            var origin = database.GetValue(EnemyAIDatabaseKey.PatrolOrigin, enemy.transform.position);
            var radius = Mathf.Max(1f, database.GetValue(EnemyAIDatabaseKey.PatrolRadius, 8f));
            var random = Random.insideUnitCircle * radius;
            var destination = origin + new Vector3(random.x, 0f, random.y);
            database.SetValue(EnemyAIDatabaseKey.PatrolDestination, destination);
            return destination;
        }

        private void ApplySlowInput(Vector3 worldDirection)
        {
            var planar = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            // 应用速度倍率（0.5倍速度 = 输入向量乘以0.5）
            var input = new Vector2(planar.x, planar.z) * speedMultiplier;
            characterActions.movement.value = input;
            characterActions.run.value = false; // 巡逻时不跑步
        }

        private void StopMovement()
        {
            characterActions.movement.value = Vector2.zero;
            characterActions.run.value = false;
        }
    }
}
