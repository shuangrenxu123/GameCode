namespace Fight.Projectile
{
    public static class ProjectileStopPolicy
    {
        public static bool ShouldStop(
            ProjectileRuntimeState state,
            IProjectileTargetProvider targetProvider,
            out ProjectileEndReason reason)
        {
            reason = ProjectileEndReason.None;
            ProjectileStopSpec stop = state.Recipe.Stop;

            if (stop.MaxLifeTime > 0f && state.AliveTime >= stop.MaxLifeTime)
            {
                reason = ProjectileEndReason.LifeTime;
                return true;
            }

            if (stop.MaxDistance > 0f && state.TravelDistance >= stop.MaxDistance)
            {
                reason = ProjectileEndReason.MaxDistance;
                return true;
            }

            if (stop.StopWhenTargetLost && state.TargetObject != null && targetProvider != null)
            {
                if (!targetProvider.TryGetTargetPosition(state.TargetObject, out _))
                {
                    reason = ProjectileEndReason.TargetLost;
                    return true;
                }
            }

            return false;
        }
    }
}
