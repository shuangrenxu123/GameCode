using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    // 空手砍柴Action
    public class HandChopWoodAction : GoapAction<string, int>
    {
        public HandChopWoodAction()
        {
            name = "Hand Chop Wood";
            cost = 1.0f;
            // Preconditions 为空，由 CheckProceduralPreCondition 检查

            // Effects: Stamina -= 2, Wood += 1
            Effects["Stamina"] = -2;
            Effects["Wood"] = 1;
        }

        public override bool CheckProceduralPreCondition(Dictionary<string, int> state)
        {
            // 检查Stamina >= 2
            return state.ContainsKey("Stamina") && state["Stamina"] >= 2;
        }

        protected override void Reset()
        {
            // 重置逻辑，如果需要
        }

        public override void PlanExecute()
        {
            Debug.Log("执行空手砍柴");
            executed = true;
        }
    }

    // 斧头砍柴Action
    public class AxeChopWoodAction : GoapAction<string, int>
    {
        public AxeChopWoodAction()
        {
            name = "Axe Chop Wood";
            cost = 0.8f;
            // Preconditions 为空，由 CheckProceduralPreCondition 检查

            // Effects: Stamina -= 1, AxeDurability -= 1, Wood += 2
            Effects["Stamina"] = -1;
            Effects["AxeDurability"] = -1;
            Effects["Wood"] = 2;
        }

        public override bool CheckProceduralPreCondition(Dictionary<string, int> state)
        {
            // 检查Stamina >= 1, AxeDurability >= 1
            int stamina = state.ContainsKey("Stamina") ? state["Stamina"] : 0;
            int axe = state.ContainsKey("AxeDurability") ? state["AxeDurability"] : 0;
            return stamina >= 1 && axe >= 1;
        }

        protected override void Reset()
        {
            // 重置逻辑
        }

        public override void PlanExecute()
        {
            // 执行逻辑：减少体力、耐久，增加木柴
            Debug.Log("执行斧头砍柴");
            executed = true;
        }
    }

    // 休息Action
    public class RestAction : GoapAction<string, int>
    {
        public RestAction()
        {
            name = "Rest";
            cost = 2.0f;
            // Preconditions 为空，由 CheckProceduralPreCondition 检查

            // Effects: Stamina += 5 (到上限10)
            Effects["Stamina"] = 5;
        }

        public override bool CheckProceduralPreCondition(Dictionary<string, int> state)
        {
            // Stamina < 10
            return state.ContainsKey("Stamina") && state["Stamina"] < 10;
        }

        protected override void Reset()
        {
            // 重置
        }

        public override void PlanExecute()
        {
            // 执行休息：恢复体力
            Debug.Log("执行休息");
            executed = true;
        }
    }

    // 农夫AI封装类，使用GOAP
    public class FarmerAI : MonoBehaviour
    {
        private GoapAgent<string, int> goapAgent;
        [SerializeField]
        private int stamina = 10; // 当前体力
        [SerializeField]
        private int wood = 0; // 当前木柴
        [SerializeField]
        private int axeDurability = 10; // 斧头耐久

        // 目标启用状态
        public bool collectWood = true; // 是否收集木柴
        public bool restStamina = true; // 是否恢复体力

        void Start()
        {
            goapAgent = new GoapAgent<string, int>();

            // 添加Actions
            goapAgent.AddAction(new HandChopWoodAction());
            goapAgent.AddAction(new AxeChopWoodAction());
            goapAgent.AddAction(new RestAction());

            // 添加Goals
            var goalCollectWood = new Dictionary<string, int>
            {
                ["Wood"] = 2 // 收集到2捆木柴
            };
            var g1 = new Goal<string, int>(goalCollectWood, 10); // 高级优先级

            var goalCollectWood2 = new Dictionary<string, int>
            {
                ["Wood"] = 1 // 收集到1捆木柴
            };
            var g3 = new Goal<string, int>(goalCollectWood2, 5); // 高级优先级


            var goalRest = new Dictionary<string, int>
            {
                ["Stamina"] = 10 // Stamina >= 10
            };
            var g2 = new Goal<string, int>(goalRest, 5); // 低优先级

            if (collectWood)
            {
                goapAgent.AddGoal(g1);
                goapAgent.AddGoal(g3);
            }
            if (restStamina) goapAgent.AddGoal(g2);

            // 初始化worldState
            UpdateWorldState();
        }

        void Update()
        {
            // TODO: 添加生命周期逻辑
            // 例如，每秒尝试规划一次
            if (Time.time - goapAgent.LastPlanTime > 1f)
            {
                goapAgent.BuildPlan();
                goapAgent.RunPlan();
                Debug.Log($"执行空手砍柴后状态: Stamina {goapAgent.WorldState["Stamina"]}, Wood {goapAgent.WorldState["Wood"]}, AxeDurability {goapAgent.WorldState["AxeDurability"]}");
            }

            // 可能更新状态，如果外部改变了
        }

        // 获取当前world state
        private Dictionary<string, int> GetWorldState()
        {
            var state = new Dictionary<string, int>
            {
                ["Stamina"] = stamina,
                ["Wood"] = wood,
                ["AxeDurability"] = axeDurability
            };
            return state;
        }

        // 执行action时的回调，更新实际变量
        // 需要hook到Actions的PlanExecute
        // 由于Actions是静态的，我们可以在这里更新
        // 但为了耦合，假设在PlanExecute中调用这个
        // public void OnHandChopWood()
        // {
        // stamina = Mathf.Max(0, stamina - 2);
        // wood += 1;
        // UpdateWorldState();
        // Debug.Log($"执行空手砍柴后状态: Stamina {stamina}, Wood {wood}, AxeDurability {axeDurability}");
        // }

        // public void OnAxeChopWood()
        // {
        // stamina = Mathf.Max(0, stamina - 1);
        // axeDurability = Mathf.Max(0, axeDurability - 1);
        // wood += 2;
        // UpdateWorldState();
        // Debug.Log($"执行斧头砍柴后状态: Stamina {stamina}, Wood {wood}, AxeDurability {axeDurability}");
        // }

        public void OnRest()
        {

        }

        private void UpdateWorldState()
        {
            goapAgent.WorldState = GetWorldState();
        }
    }
}