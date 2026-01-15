using System;
using AIBlackboard;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BT.RuntimeSerialization
{
    /// <summary>
    /// 行为树运行时驱动器：
    /// - 从导出的 Runtime JSON（TextAsset）构建运行时 BTNode 树
    /// - 可选按 Update/FixedUpdate/LateUpdate 以固定间隔 Tick
    /// - 默认仅在失败/异常时输出一次错误日志，避免刷屏
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class BTTreeRuntimeRunner : MonoBehaviour
    {
        public enum TickPhase
        {
            Update = 0,
            FixedUpdate = 1,
            LateUpdate = 2,
            Manual = 3,
        }

        [Header("Input")]
        [LabelText("运行时行为树 JSON（由编辑器导出，TextAsset）")]
        [SerializeField] TextAsset runtimeJson;

        [Header("Build")]
        [LabelText("启用组件时自动构建")]
        [SerializeField] bool buildOnEnable = true;
        [LabelText("构建失败时保留旧树（避免 AI 瞬间失效）")]
        [SerializeField] bool keepPreviousOnBuildFailure = true;

        [Header("Tick")]
        [LabelText("是否启用 Tick 驱动")]
        [SerializeField] bool tickEnabled = true;
        [LabelText("Tick 时机（Update/Fixed/Late/Manual）")]
        [SerializeField] TickPhase tickPhase = TickPhase.Update;
        [LabelText("Tick 间隔（0 表示每帧）")]
        [SerializeField, Min(0f)] float tickInterval = 0f;
        [LabelText("使用不受 TimeScale 影响的时间")]
        [SerializeField] bool useUnscaledTime = false;
        [LabelText("当结果不是 Running 时停止 Tick")]
        [SerializeField] bool stopTickWhenNotRunning = false;

        [Header("Logging")]
        [LabelText("输出错误日志（默认仅输出一次，避免刷屏）")]
        [SerializeField] bool logErrors = true;

        BTNode root;
        Blackboard blackboard;
        float nextTickTime;
        BTResult? lastResult;
        bool loggedBuildError;
        bool loggedTickError;

#if UNITY_EDITOR
        [Header("Editor")]
        [LabelText("PlayMode 下修改 Inspector 时请求重建")]
        [SerializeField] bool rebuildOnValidateInPlayMode = false;
        bool rebuildRequested;
#endif

        public TextAsset RuntimeJson
        {
            get => runtimeJson;
            set => runtimeJson = value;
        }

        public BTNode Root => root;
        public Blackboard Blackboard => blackboard;
        public BTResult? LastResult => lastResult;

        public bool TickEnabled
        {
            get => tickEnabled;
            set
            {
                if (tickEnabled == value)
                    return;

                tickEnabled = value;
                if (tickEnabled)
                    ResetTickSchedule();
            }
        }

        void OnEnable()
        {
            loggedBuildError = false;
            loggedTickError = false;

            if (buildOnEnable)
                TryBuild();
        }

        void OnDisable()
        {
            Stop(clearTree: true);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            EnsureInspectorBlackboardValueTypes();
            if (!rebuildOnValidateInPlayMode)
                return;
            if (!Application.isPlaying)
                return;

            rebuildRequested = true;
        }
#endif

        void Update()
        {
            if (tickPhase != TickPhase.Update)
                return;

            TickInternal();
        }

        void FixedUpdate()
        {
            if (tickPhase != TickPhase.FixedUpdate)
                return;

            TickInternal();
        }

        void LateUpdate()
        {
            if (tickPhase != TickPhase.LateUpdate)
                return;

            TickInternal();
        }

        /// <summary>
        /// 尝试构建行为树（使用当前 runtimeJson）。
        /// </summary>
        [ContextMenu("Build")]
        public bool TryBuild()
        {
            return TryBuild(runtimeJson);
        }

        /// <summary>
        /// 尝试构建行为树（使用指定 TextAsset）。
        /// </summary>
        public bool TryBuild(TextAsset jsonAsset)
        {
            if (jsonAsset == null || string.IsNullOrWhiteSpace(jsonAsset.text))
            {
                LogBuildErrorOnce("BTTreeRuntimeRunner: 未设置 runtimeJson 或内容为空。");
                if (!keepPreviousOnBuildFailure)
                    Stop(clearTree: true);
                return false;
            }

            BTNode newRoot = null;
            Blackboard newBlackboard = null;

            try
            {
                newRoot = BTTreeRuntimeBuilder.BuildFromJson(jsonAsset.text, out newBlackboard);
            }
            catch (Exception e)
            {
                LogBuildErrorOnce($"BTTreeRuntimeRunner: BuildFromJson 失败：{e.Message}");
                if (!keepPreviousOnBuildFailure)
                    Stop(clearTree: true);
                return false;
            }

            if (newRoot == null)
            {
                LogBuildErrorOnce("BTTreeRuntimeRunner: 构建失败，root 为 null（常见原因：rootId 无效或 typeId 未被生成工厂支持）。");
                if (!keepPreviousOnBuildFailure)
                    Stop(clearTree: true);
                return false;
            }

            // 切换到新树前，清理旧树，避免引用遗留导致的不确定行为
            Stop(clearTree: true);

            root = newRoot;
            blackboard = newBlackboard ?? new Blackboard();
            ApplyInspectorBlackboard(blackboard);
            root.Activate(blackboard);

            ResetTickSchedule();
            lastResult = null;
            loggedBuildError = false;
            loggedTickError = false;
            return true;
        }

        /// <summary>
        /// 停止并释放当前行为树引用。
        /// </summary>
        [ContextMenu("Stop")]
        public void Stop()
        {
            Stop(clearTree: true);
        }

        public void Stop(bool clearTree)
        {
            if (clearTree && root != null)
            {
                try
                {
                    root.Clear();
                }
                catch
                {
                    // 运行时稳定性优先：Clear 失败不影响 Stop
                }
            }

            root = null;
            blackboard = null;
            lastResult = null;
            nextTickTime = 0f;
        }

        /// <summary>
        /// 请求在下一次 Update/FIxedUpdate/LateUpdate 前重建（避免在 OnValidate 等频繁回调里重复构建）。
        /// </summary>
        public void RequestRebuild()
        {
#if UNITY_EDITOR
            rebuildRequested = true;
#else
            // 非编辑器环境下通常不会频繁触发，这里直接构建即可
            TryBuild();
#endif
        }

        /// <summary>
        /// 手动执行一次 Tick（当 tickPhase=Manual 时用于外部驱动）。
        /// </summary>
        public BTResult TickOnce()
        {
            return TickOnceInternal();
        }

        void TickInternal()
        {
#if UNITY_EDITOR
            if (rebuildRequested)
            {
                rebuildRequested = false;
                TryBuild();
            }
#endif

            if (!tickEnabled || root == null)
                return;

            var now = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (tickInterval > 0f && now < nextTickTime)
                return;

            if (tickInterval > 0f)
                nextTickTime = now + tickInterval;

            var result = TickOnceInternal();
            if (!lastResult.HasValue || result != lastResult.Value)
                lastResult = result;

            if (stopTickWhenNotRunning && result != BTResult.Running)
                tickEnabled = false;
        }

        BTResult TickOnceInternal()
        {
            if (root == null)
                return BTResult.Failed;

            try
            {
                return root.Tick();
            }
            catch (Exception e)
            {
                LogTickErrorOnce($"BTTreeRuntimeRunner: Tick 异常：{e.Message}");
                // 异常后默认停掉 Tick，避免持续异常导致性能与日志问题
                tickEnabled = false;
                return BTResult.Failed;
            }
        }

        void ResetTickSchedule()
        {
            var now = useUnscaledTime ? Time.unscaledTime : Time.time;
            nextTickTime = now;
        }

        void LogBuildErrorOnce(string message)
        {
            if (!logErrors || loggedBuildError)
                return;
            loggedBuildError = true;
            Debug.LogError(message, this);
        }

        void LogTickErrorOnce(string message)
        {
            if (!logErrors || loggedTickError)
                return;
            loggedTickError = true;
            Debug.LogError(message, this);
        }
    }
}
