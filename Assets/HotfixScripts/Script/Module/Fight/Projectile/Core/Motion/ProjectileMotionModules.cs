using System;

namespace Fight.Projectile
{
    public interface IProjectileMotionModule
    {
        int Order { get; }

        // 模块实例会被同一份配方下的多颗投射物复用，不要在模块字段里保存单颗投射物状态。
        void OnAttach(ProjectileRuntimeState state);
        void OnDetach(ProjectileRuntimeState state);
        void Evaluate(ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose);
    }

    public abstract class ProjectileMotionModuleBase : IProjectileMotionModule
    {
        public int Order { get; }

        protected ProjectileMotionModuleBase(int order)
        {
            Order = order;
        }

        public virtual void OnAttach(ProjectileRuntimeState state)
        {
        }

        public virtual void OnDetach(ProjectileRuntimeState state)
        {
        }

        public abstract void Evaluate(ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose);
    }

    public sealed class LinearProjectileMotion : ProjectileMotionModuleBase
    {
        readonly float speed;
        readonly float stopAfterSeconds;

        public LinearProjectileMotion(float speed, float stopAfterSeconds, int order) : base(order)
        {
            this.speed = ProjectileMath.Max(0f, speed);
            this.stopAfterSeconds = ProjectileMath.Max(0f, stopAfterSeconds);
        }

        public override void Evaluate(ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose)
        {
            if (stopAfterSeconds > 0f && state.AliveTime >= stopAfterSeconds)
            {
                pose.Velocity = ProjectileVector3.Zero;
                return;
            }

            float resolvedSpeed = speed > 0f ? speed : state.Recipe.InitialSpeed;
            ProjectileVector3 forward = ProjectileVector3.NormalizeOrForward(pose.Forward);
            pose.Forward = forward;
            pose.Velocity = forward * resolvedSpeed;
            pose.Position += pose.Velocity * context.DeltaTime;
        }
    }

    public sealed class StaticProjectileMotion : ProjectileMotionModuleBase
    {
        public StaticProjectileMotion(int order) : base(order)
        {
        }

        public override void Evaluate(ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose)
        {
            pose.Velocity = ProjectileVector3.Zero;
        }
    }

    public sealed class RoundProjectileMotion : ProjectileMotionModuleBase
    {
        readonly float angularSpeedDeg;
        readonly float radius;

        public RoundProjectileMotion(float angularSpeedDeg, float radius, int order) : base(order)
        {
            this.angularSpeedDeg = angularSpeedDeg;
            this.radius = ProjectileMath.Max(0f, radius);
        }

        public override void Evaluate(ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose)
        {
            float angle = state.AliveTime * angularSpeedDeg;
            ProjectileVector3 offset = ProjectileVector3.RotateYaw(ProjectileVector3.Forward, angle) * radius;
            ProjectileVector3 nextPosition = state.SpawnPosition + offset;
            ProjectileVector3 delta = nextPosition - pose.Position;
            pose.Position = nextPosition;
            pose.Forward = delta.SqrMagnitude > 0.0001f ? delta.Normalized : pose.Forward;
            pose.Velocity = context.DeltaTime > 0f ? delta / context.DeltaTime : ProjectileVector3.Zero;
        }
    }

    public sealed class WeakHomingProjectileMotion : ProjectileMotionModuleBase
    {
        readonly float turnSpeedDeg;
        readonly float speed;

        public WeakHomingProjectileMotion(float turnSpeedDeg, float speed, int order) : base(order)
        {
            this.turnSpeedDeg = ProjectileMath.Max(0f, turnSpeedDeg);
            this.speed = ProjectileMath.Max(0f, speed);
        }

        public override void Evaluate(ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose)
        {
            ProjectileVector3 forward = ProjectileVector3.NormalizeOrForward(pose.Forward);
            if (context.TargetProvider != null
                && state.TargetObject != null
                && context.TargetProvider.TryGetTargetPosition(state.TargetObject, out ProjectileVector3 targetPosition))
            {
                ProjectileVector3 desired = targetPosition - pose.Position;
                if (desired.SqrMagnitude > 0.0001f)
                {
                    forward = RotateTowards(forward, desired.Normalized, turnSpeedDeg * context.DeltaTime);
                }
            }

            float resolvedSpeed = speed > 0f ? speed : state.Recipe.InitialSpeed;
            pose.Forward = forward;
            pose.Velocity = forward * resolvedSpeed;
            pose.Position += pose.Velocity * context.DeltaTime;
        }

        static ProjectileVector3 RotateTowards(ProjectileVector3 current, ProjectileVector3 target, float maxDegrees)
        {
            float dot = ProjectileMath.Clamp(ProjectileVector3.Dot(current.Normalized, target.Normalized), -1f, 1f);
            float angle = (float)(Math.Acos(dot) / ProjectileMath.Deg2Rad);
            if (angle <= maxDegrees || angle <= 0.0001f)
            {
                return target.Normalized;
            }

            float t = maxDegrees / angle;
            return ProjectileVector3.Lerp(current, target, t).Normalized;
        }
    }

    public static class ProjectileMotionFactory
    {
        public static IProjectileMotionModule Create(
            ProjectileMotionType type,
            float speed,
            float stopAfterSeconds,
            float radius,
            float angularSpeed,
            float turnSpeed,
            int order)
        {
            switch (type)
            {
                case ProjectileMotionType.Static:
                    return new StaticProjectileMotion(order);
                case ProjectileMotionType.Round:
                    return new RoundProjectileMotion(angularSpeed, radius, order);
                case ProjectileMotionType.WeakHoming:
                    return new WeakHomingProjectileMotion(turnSpeed, speed, order);
                case ProjectileMotionType.Linear:
                default:
                    return new LinearProjectileMotion(speed, stopAfterSeconds, order);
            }
        }
    }
}
