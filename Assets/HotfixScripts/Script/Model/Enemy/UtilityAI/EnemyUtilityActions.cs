using AIBlackboard;
using CharacterController;
using Enemy;
using Fight;
using UnityEngine;
using UtilityAI;

namespace Enemy.AI.Utility
{
    /// <summary>
    /// 通用的敌人效用AI黑板键
    /// </summary>
    internal static class EnemyUtilityBlackboardKeys
    {
        internal static readonly BlackboardKey<Vector3> PatrolOrigin = new(EnemyAIDatabaseKey.PatrolOrigin.ToString());
        internal static readonly BlackboardKey<Vector3> PatrolDestination = new(EnemyAIDatabaseKey.PatrolDestination.ToString());
        internal static readonly BlackboardKey<float> PatrolRadius = new(EnemyAIDatabaseKey.PatrolRadius.ToString());
        internal static readonly BlackboardKey<float> SafeDistance = new(EnemyAIDatabaseKey.FleeSafeDistance.ToString());
        internal static readonly BlackboardKey<float> LowHealthThreshold = new(EnemyAIDatabaseKey.LowHealthThreshold.ToString());
        internal static readonly BlackboardKey<float> PlayerDistance = new(EnemyAIDatabaseKey.PlayerDistance.ToString());
        internal static readonly BlackboardKey<bool> PlayerVisible = new(EnemyAIDatabaseKey.PlayerVisible.ToString());
        internal static readonly BlackboardKey<CombatEntity> CombatEntity = new(EnemyAIDatabaseKey.CombatEntity.ToString());
        internal static readonly BlackboardKey<UtilityBrain> UtilityBrain = new(EnemyAIDatabaseKey.UtilityBrain.ToString());
        internal static readonly BlackboardKey<CharacterBrain> CharacterBrain = new("characterBrain");
        internal static readonly BlackboardKey<CharacterActions> CharacterActions = new("characterActions");
        internal static readonly BlackboardKey<Transform> TargetTransform = new("targetTransform");
    }

    /// <summary>
    /// 敌人行动的公共基类，负责写入CharacterActions
    /// </summary>
    public abstract class EnemyMovementActionBase : ContinuousAction
    {
        readonly string actionName;
        protected readonly Enemy enemy;
        protected readonly CharacterBrain characterBrain;
        protected readonly CharacterActions characterActions;

        protected EnemyMovementActionBase(string name, Enemy enemy, CharacterBrain characterBrain, CharacterActions characterActions)
        {
            actionName = name;
            this.enemy = enemy;
            this.characterBrain = characterBrain;
            this.characterActions = characterActions;
        }

        public override string Name => actionName;

        protected void ApplyInput(Vector3 worldDirection, bool run)
        {
            var planar = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            var input = new Vector2(planar.x, planar.z);
            characterActions.movement.value = input;
            characterActions.run.value = run;
        }

        protected void StopMovement()
        {
            characterActions.movement.value = Vector2.zero;
            characterActions.run.value = false;
        }

        public override void Exit(Blackboard blackboard)
        {
            StopMovement();
            base.Exit(blackboard);
        }

        public override void Abort(Blackboard blackboard)
        {
            StopMovement();
            base.Abort(blackboard);
        }
    }

    /// <summary>
    /// 巡逻行为：在巡逻半径范围内随机游走
    /// </summary>
    public class EnemyPatrolAction : EnemyMovementActionBase
    {
        readonly float arrivalThreshold;

        public EnemyPatrolAction(Enemy enemy, CharacterBrain characterBrain, CharacterActions characterActions, float arrivalThreshold = 0.5f)
            : base("Patrol", enemy, characterBrain, characterActions)
        {
            this.arrivalThreshold = Mathf.Max(0.1f, arrivalThreshold);
        }

        protected override ActionState OnUpdate(Blackboard blackboard)
        {
            var destination = GetOrCreateDestination(blackboard);
            Vector3 direction = destination - enemy.characterActor.transform.position;

            if (direction.magnitude <= arrivalThreshold)
            {
                // 到达巡逻点后重新生成目标
                GenerateNewDestination(blackboard);
                return ActionState.Success;
            }

            ApplyInput(direction, false);
            return ActionState.Running;
        }

        Vector3 GetOrCreateDestination(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(EnemyUtilityBlackboardKeys.PatrolDestination, out Vector3 destination))
            {
                destination = GenerateNewDestination(blackboard);
            }
            return destination;
        }

        Vector3 GenerateNewDestination(Blackboard blackboard)
        {
            var origin = blackboard.GetValue(EnemyUtilityBlackboardKeys.PatrolOrigin, enemy.transform.position);
            var radius = Mathf.Max(1f, blackboard.GetValue(EnemyUtilityBlackboardKeys.PatrolRadius, 5f));
            var random = Random.insideUnitCircle * radius;
            var destination = origin + new Vector3(random.x, 0f, random.y);
            blackboard.SetValue(EnemyUtilityBlackboardKeys.PatrolDestination, destination);
            return destination;
        }
    }

    /// <summary>
    /// 逃跑行为：远离目标直至达到安全距离
    /// </summary>
    public class EnemyFleeAction : EnemyMovementActionBase
    {
        readonly float minSafeDistance;

        public EnemyFleeAction(Enemy enemy, CharacterBrain characterBrain, CharacterActions characterActions, float minSafeDistance = 3f)
            : base("Flee", enemy, characterBrain, characterActions)
        {
            this.minSafeDistance = Mathf.Max(0.1f, minSafeDistance);
        }

        protected override ActionState OnUpdate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(EnemyUtilityBlackboardKeys.TargetTransform, out Transform target) || target == null)
            {
                StopMovement();
                return ActionState.Failed;
            }

            float safeDistance = Mathf.Max(minSafeDistance, blackboard.GetValue(EnemyUtilityBlackboardKeys.SafeDistance, minSafeDistance));
            Vector3 direction = enemy.characterActor.transform.position - target.position;
            float currentDistance = direction.magnitude;

            if (currentDistance >= safeDistance)
            {
                StopMovement();
                return ActionState.Success;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = enemy.characterActor.transform.forward * -1f;
            }

            ApplyInput(direction, true);
            return ActionState.Running;
        }
    }
}
