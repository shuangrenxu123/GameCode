using AIBlackboard;
using UnityEngine;

namespace BT.RuntimeSerialization
{
    public sealed class BTTreeRuntimeBuildTester : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] TextAsset runtimeJson;

        [Header("Behavior")]
        [SerializeField] bool buildOnStart = true;
        [SerializeField] bool tickOnUpdate = true;
        [SerializeField, Min(0f)] float tickInterval;

        BTNode root;
        Blackboard blackboard;
        float nextTickTime;
        BTResult? lastResult;

        public void Build()
        {
            if (runtimeJson == null || string.IsNullOrWhiteSpace(runtimeJson.text))
            {
                Debug.LogError("BTTreeRuntimeBuildTester: 未设置 runtimeJson 或内容为空。", this);
                return;
            }

            try
            {
                root = BTTreeRuntimeBuilder.BuildFromJson(runtimeJson.text, out blackboard);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e, this);
                root = null;
                blackboard = null;
                return;
            }

            if (root == null)
            {
                Debug.LogError("BTTreeRuntimeBuildTester: 构建失败，root 为 null（常见原因：rootId 无效/节点 typeId 未被工厂支持）。", this);
                return;
            }

            root.Activate(blackboard ?? new Blackboard());
            nextTickTime = Time.time;
            lastResult = null;

            Debug.Log($"BTTreeRuntimeBuildTester: 构建完成并已 Activate。root={root.GetType().FullName}", this);
        }

        void Start()
        {
            if (buildOnStart)
                Build();
        }

        void Update()
        {
            if (!tickOnUpdate || root == null)
                return;

            if (tickInterval > 0f)
            {
                if (Time.time < nextTickTime)
                    return;
                nextTickTime = Time.time + tickInterval;
            }

            var result = root.Tick();
            if (!lastResult.HasValue || result != lastResult.Value)
            {
                lastResult = result;
                Debug.Log($"BTTreeRuntimeBuildTester: Tick => {result}", this);
            }
        }
    }
}
