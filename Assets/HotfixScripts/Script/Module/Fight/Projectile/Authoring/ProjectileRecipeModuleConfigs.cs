using System;
using Fight.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.Authoring
{
    public enum ProjectileDetectionShapeKind
    {
        None = 0,
        SphereCast = 1,
        OverlapSphere = 2,
        OverlapBox = 3,
        ConeOverlap = 4,
        RayFan = 5,
    }

    [Serializable]
    public sealed class ProjectileMotionConfig
    {
        [SerializeField, LabelText("启用")]
        public bool enabled = true;

        [SerializeField, LabelText("执行顺序")]
        [MinValue(0)]
        public int order = 100;

        [SerializeField, LabelText("运动类型")]
        public ProjectileMotionType type = ProjectileMotionType.Linear;

        [SerializeField, LabelText("速度")]
        [MinValue(0f)]
        public float speed = 20f;

        [SerializeField, LabelText("停止前飞行时间（0表示不停止）")]
        [MinValue(0f)]
        public float stopAfterSeconds;

        [SerializeField, LabelText("环绕半径")]
        [MinValue(0f)]
        public float roundRadius = 2f;

        [SerializeField, LabelText("环绕角速度")]
        public float angularSpeed = 180f;

        [SerializeField, LabelText("追踪转向角速度")]
        [MinValue(0f)]
        public float homingTurnSpeed = 90f;
    }

    [Serializable]
    public sealed class ProjectileDetectionConfig
    {
        [SerializeField, LabelText("检测类型")]
        public ProjectileDetectionShapeKind type = ProjectileDetectionShapeKind.SphereCast;

        [SerializeField, LabelText("检测半径")]
        [MinValue(0f)]
        public float radius = 0.25f;

        [SerializeField, LabelText("检测角度")]
        [Range(0f, 360f)]
        public float angle = 60f;

        [SerializeField, LabelText("检测射线数量")]
        [MinValue(1)]
        public int rayCount = 5;

        [SerializeField, LabelText("盒体宽度")]
        [MinValue(0f)]
        public float boxWidth = 1f;

        [SerializeField, LabelText("盒体高度")]
        [MinValue(0f)]
        public float boxHeight = 1f;

        [SerializeField, LabelText("盒体长度")]
        [MinValue(0f)]
        public float boxLength = 2f;

        [SerializeField, LabelText("单次检测命中上限")]
        [MinValue(1)]
        public int maxHits = 16;
    }

    [Serializable]
    public sealed class ProjectileHitEffectConfig
    {
        [SerializeField, LabelText("启用")]
        public bool enabled = true;

        [SerializeField, LabelText("执行顺序")]
        [MinValue(0)]
        public int order = 300;

        [SerializeField, LabelText("效果类型")]
        public ProjectileHitEffectType type = ProjectileHitEffectType.Damage;

        [SerializeField, LabelText("基础数值（0 使用命中配置基础数值）")]
        [MinValue(0)]
        public int baseValue;

        [SerializeField, LabelText("Buff ID")]
        public BuffId buffId = BuffId.None;
    }

    public enum ProjectileHitEffectType
    {
        Damage = 0,
        Regeneration = 1,
        AddBuff = 2,
    }

    [Serializable]
    public sealed class ProjectileHitConfig
    {
        [SerializeField, LabelText("结算类型")]
        public ProjectileHitResolveType resolveType = ProjectileHitResolveType.Damage;

        [SerializeField, LabelText("结算模式")]
        public ProjectileHitResolveMode resolveMode = ProjectileHitResolveMode.Continuous;

        [SerializeField, LabelText("基础数值")]
        [MinValue(0)]
        public int baseValue = 10;

        [SerializeField, LabelText("穿透次数")]
        [MinValue(0)]
        public int pierceCount;

        [SerializeField, LabelText("命中间隔")]
        [MinValue(0f)]
        public float hitInterval = 0.5f;

        [SerializeField, LabelText("同目标命中冷却")]
        [MinValue(0f)]
        public float targetHitCooldown;

        [SerializeField, LabelText("忽略发射者")]
        public bool ignoreOwner = true;

        [SerializeField, LabelText("组合命中效果")]
        public ProjectileHitEffectConfig[] effects = Array.Empty<ProjectileHitEffectConfig>();
    }

    [Serializable]
    public sealed class ProjectileStopConfig
    {
        [SerializeField, LabelText("最大生命周期")]
        [MinValue(0f)]
        public float maxLifeTime = 5f;

        [SerializeField, LabelText("最大飞行距离")]
        [MinValue(0f)]
        public float maxDistance = 60f;

        [SerializeField, LabelText("命中目标后销毁")]
        public bool destroyOnTargetHit = true;

        [SerializeField, LabelText("目标丢失时停止")]
        public bool stopWhenTargetLost;
    }

    [Serializable]
    public sealed class ProjectileSpawnConfig
    {
        [SerializeField, LabelText("发射数量")]
        [MinValue(1)]
        public int spawnCount = 1;

        [SerializeField, LabelText("相邻散射角度")]
        public float spawnAngle;

        [SerializeField, LabelText("整批共用随机角")]
        public bool batchRandomAngle;

        [SerializeField, LabelText("随机角最小值")]
        public float randomAngleMin;

        [SerializeField, LabelText("随机角最大值")]
        public float randomAngleMax;

        public ProjectileSpawnSettings ToSpec()
        {
            return new ProjectileSpawnSettings
            {
                SpawnCount = spawnCount,
                SpawnAngle = spawnAngle,
                BatchRandomAngle = batchRandomAngle,
                RandomAngleMin = randomAngleMin,
                RandomAngleMax = randomAngleMax,
            }.Normalized();
        }
    }
}
