using System.Collections.Generic;

namespace Fight.Projectile
{
    public sealed class ProjectileRuntimeState
    {
        public ProjectileHandle Handle;
        public ProjectileRecipeSpec Recipe;
        public ProjectilePose Pose;
        public ProjectileVector3 SpawnPosition;
        public int ProjectileAssetId;
        public int SpawnBatchCount;
        public int SpawnBatchIndex;
        public int RandomSeed;
        public int OwnerId;
        public int TargetId;
        public object OwnerObject;
        public object TargetObject;
        public object InstanceObject;
        public bool CanResolveHit;
        public int BaseValue;
        public int RemainingPierceCount;
        public int TotalHitCount;
        public float AliveTime;
        public float TravelDistance;
        public float NextHitResolveTime;
        public bool IsRunning;
        public ProjectileEndReason EndReason;
        public Dictionary<int, float> TargetHitCooldownExpireTimes;

        public void Reset()
        {
            Handle = ProjectileHandle.Invalid;
            Recipe = null;
            Pose = default;
            SpawnPosition = ProjectileVector3.Zero;
            ProjectileAssetId = 0;
            SpawnBatchCount = 1;
            SpawnBatchIndex = 0;
            RandomSeed = 1;
            OwnerId = 0;
            TargetId = 0;
            OwnerObject = null;
            TargetObject = null;
            InstanceObject = null;
            CanResolveHit = false;
            BaseValue = 0;
            RemainingPierceCount = 0;
            TotalHitCount = 0;
            AliveTime = 0f;
            TravelDistance = 0f;
            NextHitResolveTime = 0f;
            IsRunning = false;
            EndReason = ProjectileEndReason.None;
            TargetHitCooldownExpireTimes?.Clear();
        }
    }
}
