using Fight;
using UnityEngine;

namespace HTN
{
    public class GameRoot : MonoBehaviour
    {
        protected DomainBase domain;
        protected Planner Planner;
        protected PlanRunner PlanRunner;
        protected CombatEntity entity;
        protected WorldState ws;
        public void Start()
        {
            WorldState ws = new WorldState();
            entity = GetComponent<CombatEntity>();
            //在其中添加具体的任务
            domain.Init();
            domain.BuildWorldState();
            //初始化预测器
            Planner = new Planner();
            Planner.Init(ws, domain);
            //初始化 执行器
            PlanRunner = new PlanRunner(ws);
        }
        private void Update()
        {
            Planner.BuildPlan();
            PlanRunner.RunPlan(Planner.GetFinalTask());
        }
    }
}