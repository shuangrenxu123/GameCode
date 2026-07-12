using System;

namespace Fight.Projectile
{
    public sealed class ProjectileRecipeSpec
    {
        public readonly int ProjectileAssetId;
        public readonly float InitialSpeed;
        public readonly IProjectileMotionModule[] MotionModules;
        public readonly ProjectileDetectionSpec Detection;
        public readonly ProjectileHitSpec Hit;
        public readonly IProjectileHitResolver HitResolver;
        public readonly ProjectileStopSpec Stop;

        public ProjectileRecipeSpec(
            int projectileAssetId,
            float initialSpeed,
            IProjectileMotionModule[] motionModules,
            in ProjectileDetectionSpec detection,
            in ProjectileHitSpec hit,
            IProjectileHitResolver hitResolver,
            in ProjectileStopSpec stop)
        {
            ProjectileAssetId = projectileAssetId;
            InitialSpeed = ProjectileMath.Max(0f, initialSpeed);
            MotionModules = motionModules ?? Array.Empty<IProjectileMotionModule>();
            Detection = detection.Normalized();
            Hit = hit.Normalized();
            HitResolver = hitResolver;
            Stop = stop.Normalized();
        }
    }

    public struct ProjectileDetectionSpec
    {
        public IProjectileDetectionShape Shape;
        public int MaxHits;

        public bool HasShape => Shape != null;

        public ProjectileDetectionSpec Normalized()
        {
            MaxHits = ProjectileMath.Max(1, MaxHits);
            return this;
        }
    }

    public interface IProjectileDetectionShape
    {
    }

    public struct ProjectileHitSpec
    {
        public ProjectileHitResolveType ResolveType;
        public ProjectileHitResolveMode ResolveMode;
        public int BaseValue;
        public int PierceCount;
        public float HitInterval;
        public float TargetHitCooldown;
        public bool IgnoreOwner;

        public ProjectileHitSpec Normalized()
        {
            BaseValue = ProjectileMath.Max(0, BaseValue);
            PierceCount = ProjectileMath.Max(0, PierceCount);
            HitInterval = ProjectileMath.Max(0f, HitInterval);
            TargetHitCooldown = ProjectileMath.Max(0f, TargetHitCooldown);
            return this;
        }
    }

    public struct ProjectileStopSpec
    {
        public float MaxLifeTime;
        public float MaxDistance;
        public bool DestroyOnTargetHit;
        public bool StopWhenTargetLost;

        public ProjectileStopSpec Normalized()
        {
            MaxLifeTime = ProjectileMath.Max(0f, MaxLifeTime);
            MaxDistance = ProjectileMath.Max(0f, MaxDistance);
            return this;
        }
    }

    public struct ProjectileSpawnSettings
    {
        public int SpawnCount;
        public float SpawnAngle;
        public bool BatchRandomAngle;
        public float RandomAngleMin;
        public float RandomAngleMax;

        public ProjectileSpawnSettings Normalized()
        {
            SpawnCount = ProjectileMath.Max(1, SpawnCount);
            if (RandomAngleMin > RandomAngleMax)
            {
                float temp = RandomAngleMin;
                RandomAngleMin = RandomAngleMax;
                RandomAngleMax = temp;
            }

            return this;
        }

        public static ProjectileSpawnSettings Single => new ProjectileSpawnSettings
        {
            SpawnCount = 1,
        };
    }
}
