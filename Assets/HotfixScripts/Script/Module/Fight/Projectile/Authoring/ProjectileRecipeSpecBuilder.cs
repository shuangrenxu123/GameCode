using System;
using System.Collections.Generic;
using Fight.Projectile;
using Fight.Projectile.UnityAdapter;

namespace Fight.Projectile.Authoring
{
    public static class ProjectileRecipeSpecBuilder
    {
        public static ProjectileRecipeSpec Build(
            int projectileAssetId,
            float initialSpeed,
            IReadOnlyList<ProjectileMotionConfig> motionConfigs,
            ProjectileDetectionConfig detectionConfig,
            ProjectileHitConfig hitConfig,
            ProjectileStopConfig stopConfig)
        {
            IProjectileMotionModule[] motions = BuildMotionModules(motionConfigs, initialSpeed);
            ProjectileDetectionSpec detection = BuildDetectionSpec(detectionConfig);
            var hit = new ProjectileHitSpec
            {
                ResolveType = hitConfig != null ? hitConfig.resolveType : ProjectileHitResolveType.None,
                ResolveMode = hitConfig != null ? hitConfig.resolveMode : ProjectileHitResolveMode.Continuous,
                BaseValue = hitConfig != null ? hitConfig.baseValue : 0,
                PierceCount = hitConfig != null ? hitConfig.pierceCount : 0,
                HitInterval = hitConfig != null ? hitConfig.hitInterval : 0f,
                TargetHitCooldown = hitConfig != null ? hitConfig.targetHitCooldown : 0f,
                IgnoreOwner = hitConfig == null || hitConfig.ignoreOwner,
            };
            var stop = new ProjectileStopSpec
            {
                MaxLifeTime = stopConfig != null ? stopConfig.maxLifeTime : 0f,
                MaxDistance = stopConfig != null ? stopConfig.maxDistance : 0f,
                DestroyOnTargetHit = stopConfig == null || stopConfig.destroyOnTargetHit,
                StopWhenTargetLost = stopConfig != null && stopConfig.stopWhenTargetLost,
            };

            return new ProjectileRecipeSpec(
                projectileAssetId,
                initialSpeed,
                motions,
                in detection,
                in hit,
                in stop);
        }

        static ProjectileDetectionSpec BuildDetectionSpec(ProjectileDetectionConfig detectionConfig)
        {
            return new ProjectileDetectionSpec
            {
                Shape = BuildDetectionShape(detectionConfig),
                MaxHits = detectionConfig != null ? detectionConfig.maxHits : 1,
            };
        }

        static IProjectileDetectionShape BuildDetectionShape(ProjectileDetectionConfig config)
        {
            if (config == null)
            {
                return null;
            }

            switch (config.type)
            {
                case ProjectileDetectionShapeKind.SphereCast:
                    return new SphereCastProjectileDetectionShape(config.radius);
                case ProjectileDetectionShapeKind.OverlapSphere:
                    return new OverlapSphereProjectileDetectionShape(config.radius);
                case ProjectileDetectionShapeKind.OverlapBox:
                    return new OverlapBoxProjectileDetectionShape(
                        config.boxWidth,
                        config.boxHeight,
                        config.boxLength);
                case ProjectileDetectionShapeKind.ConeOverlap:
                    return new ConeOverlapProjectileDetectionShape(config.radius, config.angle);
                case ProjectileDetectionShapeKind.RayFan:
                    return new RayFanProjectileDetectionShape(
                        config.radius,
                        config.angle,
                        config.rayCount);
                case ProjectileDetectionShapeKind.None:
                default:
                    return null;
            }
        }

        static IProjectileMotionModule[] BuildMotionModules(
            IReadOnlyList<ProjectileMotionConfig> motionConfigs,
            float defaultSpeed)
        {
            var modules = new List<IProjectileMotionModule>(4);
            if (motionConfigs != null)
            {
                for (int i = 0; i < motionConfigs.Count; i++)
                {
                    ProjectileMotionConfig config = motionConfigs[i];
                    if (config == null || !config.enabled)
                    {
                        continue;
                    }

                    modules.Add(ProjectileMotionFactory.Create(
                        config.type,
                        config.speed > 0f ? config.speed : defaultSpeed,
                        config.stopAfterSeconds,
                        config.roundRadius,
                        config.angularSpeed,
                        config.homingTurnSpeed,
                        config.order));
                }
            }

            if (modules.Count == 0)
            {
                modules.Add(new LinearProjectileMotion(defaultSpeed, 0f, 100));
            }

            modules.Sort((a, b) => a.Order.CompareTo(b.Order));
            return modules.ToArray();
        }
    }
}
