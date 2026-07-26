using System.Collections.Generic;
using GOAP;
using UnityEngine;

namespace GOAP.Demo
{
    public sealed class LumberjackGoapDemo : MonoBehaviour
    {
        [Header("场景引用")]
        [SerializeField] private Transform lumberjack;
        [SerializeField] private Transform primaryTree;
        [SerializeField] private Transform backupTree;
        [SerializeField] private Transform primaryWorkbench;
        [SerializeField] private Transform backupWorkbench;

        [Header("执行参数")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.05f;
        [SerializeField, Min(0.05f)] private float chopDuration = 0.35f;
        [SerializeField, Min(0.05f)] private float craftDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float retryPlanInterval = 0.5f;

        [Header("运行时分支开关")]
        [SerializeField] private bool primaryTreeAvailable = true;
        [SerializeField] private bool backupTreeAvailable = true;
        [SerializeField] private bool primaryWorkbenchAvailable = true;
        [SerializeField] private bool backupWorkbenchAvailable = true;

        private GoapAgent<LumberjackStateKey, int> agent;
        private float nextPlanRetryTime;
        private string currentStatus = "等待初始化";
        private int completedCycles;
        private int nextActionIndex;
        private bool initialized;

        public bool PrimaryTreeAvailable => primaryTreeAvailable;
        public bool BackupTreeAvailable => backupTreeAvailable;
        public bool PrimaryWorkbenchAvailable => primaryWorkbenchAvailable;
        public bool BackupWorkbenchAvailable => backupWorkbenchAvailable;
        public int CompletedCycles => completedCycles;

        public void Configure(
            Transform lumberjackTransform,
            Transform primaryTreeTransform,
            Transform backupTreeTransform,
            Transform primaryWorkbenchTransform,
            Transform backupWorkbenchTransform)
        {
            lumberjack = lumberjackTransform;
            primaryTree = primaryTreeTransform;
            backupTree = backupTreeTransform;
            primaryWorkbench = primaryWorkbenchTransform;
            backupWorkbench = backupWorkbenchTransform;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (agent.WorldState[LumberjackStateKey.CycleCompleted] == 1)
            {
                completedCycles++;
                agent.WorldState[LumberjackStateKey.CycleCompleted] = 0;
                currentStatus = $"第 {completedCycles} 轮完成，开始下一轮";
                TryBuildPlan(true);
                return;
            }

            agent.RunPlan();

            if (Time.time >= nextPlanRetryTime)
            {
                TryBuildPlan(false);
                nextPlanRetryTime = Time.time + retryPlanInterval;
            }
        }

        private void Initialize()
        {
            if (lumberjack == null ||
                primaryTree == null ||
                backupTree == null ||
                primaryWorkbench == null ||
                backupWorkbench == null)
            {
                currentStatus = "场景引用不完整，无法初始化";
                Debug.LogError($"[GOAP Demo] {currentStatus}", this);
                return;
            }

            agent = new GoapAgent<LumberjackStateKey, int>();
            agent.WorldState[LumberjackStateKey.Location] = (int)LumberjackLocation.Start;
            agent.WorldState[LumberjackStateKey.WoodCount] = 0;
            agent.WorldState[LumberjackStateKey.AxeDurability] = 10;
            agent.WorldState[LumberjackStateKey.CycleCompleted] = 0;

            AddTreeActions(
                "主树",
                primaryTree,
                LumberjackLocation.PrimaryTree,
                () => primaryTreeAvailable,
                1f,
                1f);
            AddTreeActions(
                "备用树",
                backupTree,
                LumberjackLocation.BackupTree,
                () => backupTreeAvailable,
                2f,
                1.3f);

            AddWorkbenchActions(
                "主工作台",
                primaryWorkbench,
                LumberjackLocation.PrimaryWorkbench,
                () => primaryWorkbenchAvailable,
                1f,
                1f);
            AddWorkbenchActions(
                "备用工作台",
                backupWorkbench,
                LumberjackLocation.BackupWorkbench,
                () => backupWorkbenchAvailable,
                3f,
                1.5f);

            agent.AddGoal(new Goal<LumberjackStateKey, int>(
                new Dictionary<LumberjackStateKey, int>
                {
                    [LumberjackStateKey.CycleCompleted] = 1
                },
                100));

            initialized = true;
            TryBuildPlan(true);
        }

        private void AddTreeActions(
            string displayName,
            Transform tree,
            LumberjackLocation location,
            System.Func<bool> isAvailable,
            float moveCost,
            float chopCost)
        {
            agent.AddAction(new LumberjackMoveAction(
                $"移动到{displayName}",
                moveCost,
                lumberjack,
                tree,
                location,
                isAvailable,
                moveSpeed,
                arrivalDistance,
                Report,
                NextActionMask()));

            for (int woodCount = 0; woodCount < 5; woodCount++)
            {
                agent.AddAction(new LumberjackChopAction(
                    $"在{displayName}砍树 {woodCount + 1}/5",
                    chopCost,
                    location,
                    woodCount,
                    isAvailable,
                    chopDuration,
                    Report,
                    NextActionMask()));
            }
        }

        private void AddWorkbenchActions(
            string displayName,
            Transform workbench,
            LumberjackLocation location,
            System.Func<bool> isAvailable,
            float moveCost,
            float craftCost)
        {
            agent.AddAction(new LumberjackMoveAction(
                $"移动到{displayName}",
                moveCost,
                lumberjack,
                workbench,
                location,
                isAvailable,
                moveSpeed,
                arrivalDistance,
                Report,
                NextActionMask()));

            agent.AddAction(new LumberjackCraftAxeAction(
                $"在{displayName}制作斧头",
                craftCost,
                location,
                isAvailable,
                craftDuration,
                Report,
                NextActionMask()));
        }

        private ulong NextActionMask()
        {
            if (nextActionIndex >= 64)
            {
                throw new System.InvalidOperationException(
                    "单个 GOAP Agent 最多支持 64 个逻辑 Action。");
            }

            return 1UL << nextActionIndex++;
        }

        private void TryBuildPlan(bool force)
        {
            bool success = force ? agent.ForceBuildPlan() : agent.BuildPlan(false);
            if (!success && force)
            {
                currentStatus = "当前没有可执行计划，请至少启用一棵树和一个工作台";
            }
        }

        private void Report(string message)
        {
            currentStatus = message;
            Debug.Log($"[GOAP Demo] {message}", this);
        }

        private void SetAvailability(ref bool field, bool value, string targetName)
        {
            if (field == value)
            {
                return;
            }

            field = value;
            currentStatus = $"{targetName}已{(value ? "启用" : "停用")}，触发重规划";
            if (initialized)
            {
                TryBuildPlan(true);
            }
        }

        private void OnGUI()
        {
            const int width = 390;
            GUILayout.BeginArea(new Rect(15, 15, width, 260), GUI.skin.box);
            GUILayout.Label("GOAP 伐木工循环演示");
            GUILayout.Label($"状态：{currentStatus}");
            GUILayout.Label($"完成循环：{completedCycles}");

            if (initialized)
            {
                GUILayout.Label(
                    $"木头：{agent.WorldState[LumberjackStateKey.WoodCount]}/5   " +
                    $"斧头耐久：{agent.WorldState[LumberjackStateKey.AxeDurability]}/10");
            }

            bool newPrimaryTree = GUILayout.Toggle(primaryTreeAvailable, "主树可用（低 Cost）");
            bool newBackupTree = GUILayout.Toggle(backupTreeAvailable, "备用树可用（高 Cost）");
            bool newPrimaryWorkbench = GUILayout.Toggle(
                primaryWorkbenchAvailable,
                "主工作台可用（低 Cost）");
            bool newBackupWorkbench = GUILayout.Toggle(
                backupWorkbenchAvailable,
                "备用工作台可用（高 Cost）");

            SetAvailability(ref primaryTreeAvailable, newPrimaryTree, "主树");
            SetAvailability(ref backupTreeAvailable, newBackupTree, "备用树");
            SetAvailability(ref primaryWorkbenchAvailable, newPrimaryWorkbench, "主工作台");
            SetAvailability(ref backupWorkbenchAvailable, newBackupWorkbench, "备用工作台");

            GUILayout.EndArea();
        }
    }
}
