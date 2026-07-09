using System;
using Fight.Projectile;
using UnityEngine;

namespace Fight.Projectile.UnityAdapter
{
    public sealed class UnityProjectileRuntimeRunner : MonoBehaviour
    {
        readonly ProjectileRuntimeState state = new ProjectileRuntimeState();

        ProjectileWorldServices services;
        ProjectileHitProcessor hitProcessor;
        ProjectileRawHit[] rawHits;
        ProjectileHit[] hits;
        bool hasNotifiedStopped;

        public event Action<UnityProjectileRuntimeRunner> Launched;
        public event Action<UnityProjectileRuntimeRunner, ProjectileEndReason> Stopped;
        public event Action<UnityProjectileRuntimeRunner, ProjectileHitContext> HitResolved;

        public ProjectileRuntimeState RuntimeState => state;
        public bool IsRunning => state.IsRunning;

        void Update()
        {
            Tick(Time.deltaTime);
        }

        void OnDisable()
        {
            if (state.IsRunning)
            {
                StopInternal(ProjectileEndReason.ManualStop);
            }
        }

        public void Launch(
            in ProjectileRunnerLaunchRequest request,
            ProjectileWorldServices worldServices,
            int maxHitsPerQuery)
        {
            if (request.Recipe == null)
            {
                return;
            }

            if (state.IsRunning)
            {
                StopInternal(ProjectileEndReason.ManualStop);
            }

            services = worldServices ?? new ProjectileWorldServices();
            hitProcessor = new ProjectileHitProcessor(services.HitFilters, services.HitResponses);
            EnsureHitBuffers(ResolveHitCapacity(request.Recipe.Detection.MaxHits, maxHitsPerQuery));
            InitializeState(in request);

            for (int i = 0; i < state.Recipe.MotionModules.Length; i++)
            {
                state.Recipe.MotionModules[i]?.OnAttach(state);
            }

            ApplyPoseToTransform();
            ResolveLaunchHitWindow();
            if (state.IsRunning)
            {
                Launched?.Invoke(this);
            }
        }

        public void Tick(float deltaTime)
        {
            if (!state.IsRunning || deltaTime <= 0f)
            {
                return;
            }

            ProjectilePose previousPose = state.Pose;
            state.AliveTime += deltaTime;

            var preMotionTrace = new ProjectileTrace(
                previousPose.Position,
                previousPose.Position,
                previousPose.Forward,
                previousPose.Forward);
            var preMotionContext = new ProjectileFrameContext(
                deltaTime,
                state.AliveTime,
                in previousPose,
                in state.Pose,
                in preMotionTrace,
                services.TargetProvider);

            for (int i = 0; i < state.Recipe.MotionModules.Length; i++)
            {
                state.Recipe.MotionModules[i]?.Evaluate(state, in preMotionContext, ref state.Pose);
            }

            state.TravelDistance += ProjectileVector3.Distance(previousPose.Position, state.Pose.Position);
            var trace = new ProjectileTrace(
                previousPose.Position,
                state.Pose.Position,
                previousPose.Forward,
                state.Pose.Forward);
            var frameContext = new ProjectileFrameContext(
                deltaTime,
                state.AliveTime,
                in previousPose,
                in state.Pose,
                in trace,
                services.TargetProvider);

            ApplyPoseToTransform();
            if (ShouldResolveHit() && ResolveHitWindow(in frameContext))
            {
                return;
            }

            if (ProjectileStopPolicy.ShouldStop(state, services.TargetProvider, out ProjectileEndReason reason))
            {
                StopInternal(reason);
            }
        }

        public void Stop(ProjectileEndReason reason)
        {
            StopInternal(reason);
        }

        void InitializeState(in ProjectileRunnerLaunchRequest request)
        {
            ProjectileRuntimeContext runtimeContext = request.RuntimeContext.Normalized();
            state.Reset();
            hasNotifiedStopped = false;
            state.Handle = request.Handle;
            state.Recipe = request.Recipe;
            state.ProjectileAssetId = request.Recipe.ProjectileAssetId;
            state.SpawnPosition = request.SpawnPosition;
            state.Pose = ProjectilePose.Create(
                request.SpawnPosition,
                request.Direction,
                request.Recipe.InitialSpeed * runtimeContext.SpeedMultiplier);
            state.SpawnBatchCount = ProjectileMath.Max(1, request.SpawnBatchCount);
            state.SpawnBatchIndex = request.SpawnBatchIndex;
            state.RandomSeed = request.RandomSeed == 0 ? 1 : request.RandomSeed;
            state.OwnerId = runtimeContext.OwnerId;
            state.TargetId = runtimeContext.TargetId;
            state.OwnerObject = runtimeContext.OwnerObject;
            state.TargetObject = runtimeContext.TargetObject;
            state.InstanceObject = gameObject;
            state.CanResolveHit = runtimeContext.CanResolveHit;
            state.BaseValue = runtimeContext.BaseValueOverride > 0
                ? runtimeContext.BaseValueOverride
                : request.Recipe.Hit.BaseValue;
            state.RemainingPierceCount = request.Recipe.Hit.PierceCount;
            state.IsRunning = true;
            state.EndReason = ProjectileEndReason.None;
            state.NextHitResolveTime = 0f;
            if (request.Recipe.Hit.TargetHitCooldown > 0f && state.TargetHitCooldownExpireTimes == null)
            {
                state.TargetHitCooldownExpireTimes = new System.Collections.Generic.Dictionary<int, float>(8);
            }
        }

        bool ShouldResolveHit()
        {
            switch (state.Recipe.Hit.ResolveMode)
            {
                case ProjectileHitResolveMode.Continuous:
                    return true;
                case ProjectileHitResolveMode.Periodic:
                    return state.Recipe.Hit.HitInterval > 0f && state.AliveTime >= state.NextHitResolveTime;
                case ProjectileHitResolveMode.OnLaunchOnly:
                default:
                    return false;
            }
        }

        void ResolveLaunchHitWindow()
        {
            if (state.Recipe.Hit.ResolveMode != ProjectileHitResolveMode.OnLaunchOnly
                && state.Recipe.Hit.ResolveMode != ProjectileHitResolveMode.Periodic)
            {
                return;
            }

            var trace = new ProjectileTrace(
                state.Pose.Position,
                state.Pose.Position,
                state.Pose.Forward,
                state.Pose.Forward);
            var frameContext = new ProjectileFrameContext(
                0f,
                state.AliveTime,
                in state.Pose,
                in state.Pose,
                in trace,
                services.TargetProvider);
            ResolveHitWindow(in frameContext);
        }

        bool ResolveHitWindow(in ProjectileFrameContext frameContext)
        {
            if (!state.Recipe.Detection.HasShape || services.Query == null)
            {
                ScheduleNextHitWindow();
                return false;
            }

            int capacity = ResolveHitCapacity(state.Recipe.Detection.MaxHits, rawHits.Length);
            EnsureHitBuffers(capacity);
            var queryRequest = new ProjectileQueryRequest(
                in state.Recipe.Detection,
                in frameContext.Trace,
                in state.Pose,
                state.OwnerId,
                state.OwnerObject);
            int rawCount = services.Query.Query(in queryRequest, rawHits);
            int hitCount = ProjectileHitAccumulator.Accumulate(rawHits, rawCount, hits);

            for (int i = 0; i < hitCount; i++)
            {
                ProjectileHit hit = hits[i];
                ProjectileHitProcessResult result = hitProcessor.Process(
                    state,
                    in hit,
                    ref state.Pose,
                    services.CombatResolver);
                if (!result.Passed)
                {
                    continue;
                }

                HitResolved?.Invoke(this, result.Context);
                ApplyPoseToTransform();
                if (result.ShouldStop)
                {
                    StopInternal(result.EndReason);
                    return true;
                }
            }

            ScheduleNextHitWindow();
            return false;
        }

        void ScheduleNextHitWindow()
        {
            if (state.Recipe.Hit.ResolveMode != ProjectileHitResolveMode.Periodic
                || state.Recipe.Hit.HitInterval <= 0f)
            {
                return;
            }

            state.NextHitResolveTime = state.AliveTime + state.Recipe.Hit.HitInterval;
        }

        void StopInternal(ProjectileEndReason reason)
        {
            if (!state.IsRunning)
            {
                return;
            }

            state.IsRunning = false;
            state.EndReason = reason;
            for (int i = state.Recipe.MotionModules.Length - 1; i >= 0; i--)
            {
                state.Recipe.MotionModules[i]?.OnDetach(state);
            }

            NotifyStopped(reason);
        }

        void NotifyStopped(ProjectileEndReason reason)
        {
            if (hasNotifiedStopped)
            {
                return;
            }

            hasNotifiedStopped = true;
            Stopped?.Invoke(this, reason);
        }

        void ApplyPoseToTransform()
        {
            transform.position = state.Pose.Position.ToUnity();
            transform.rotation = state.Pose.Forward.ToUnityRotation();
            transform.localScale = state.Pose.Scale.ToUnity();
        }

        void EnsureHitBuffers(int capacity)
        {
            capacity = ProjectileMath.Max(1, capacity);
            if (rawHits == null || rawHits.Length < capacity)
            {
                rawHits = new ProjectileRawHit[capacity];
            }

            if (hits == null || hits.Length < capacity)
            {
                hits = new ProjectileHit[capacity];
            }
        }

        static int ResolveHitCapacity(int recipeMaxHits, int maxHitsPerQuery)
        {
            int capacity = ProjectileMath.Max(1, recipeMaxHits);
            maxHitsPerQuery = ProjectileMath.Max(1, maxHitsPerQuery);
            return capacity < maxHitsPerQuery ? capacity : maxHitsPerQuery;
        }
    }
}
