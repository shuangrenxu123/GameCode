using System.Collections.Generic;
using Fight.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.Authoring
{
    [CreateAssetMenu(menuName = "Fight/Projectile/投射物配方", fileName = "ProjectileRecipe")]
    public sealed class ProjectileRecipeSO : ScriptableObject
    {
        [SerializeField, LabelText("投射物资产ID")]
        [MinValue(0)]
        int projectileAssetId;

        [SerializeField, LabelText("初始速度")]
        [MinValue(0f)]
        float initialSpeed = 20f;

        [SerializeField, LabelText("生成配置")]
        ProjectileSpawnConfig spawnConfig = new ProjectileSpawnConfig();

        [SerializeField, LabelText("检测配置")]
        ProjectileDetectionConfig detectionConfig = new ProjectileDetectionConfig();

        [SerializeField, LabelText("命中配置")]
        ProjectileHitConfig hitConfig = new ProjectileHitConfig();

        [SerializeField, LabelText("停止配置")]
        ProjectileStopConfig stopConfig = new ProjectileStopConfig();

        [SerializeField, LabelText("运动模块")]
        List<ProjectileMotionConfig> motionConfigs = new List<ProjectileMotionConfig>
        {
            new ProjectileMotionConfig(),
        };

        ProjectileRecipeSpec cachedSpec;

        public ProjectileSpawnSettings SpawnSettings => spawnConfig != null
            ? spawnConfig.ToSpec()
            : ProjectileSpawnSettings.Single;

        public ProjectileRecipeSpec BuildSpec()
        {
            if (cachedSpec != null)
            {
                return cachedSpec;
            }

            cachedSpec = ProjectileRecipeSpecBuilder.Build(
                projectileAssetId,
                initialSpeed,
                motionConfigs,
                detectionConfig,
                hitConfig,
                stopConfig);
            return cachedSpec;
        }

        void OnValidate()
        {
            cachedSpec = null;
        }
    }
}
