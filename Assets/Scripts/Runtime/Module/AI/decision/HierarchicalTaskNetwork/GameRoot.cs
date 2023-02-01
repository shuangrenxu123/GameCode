using System;
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
        public string AIName;
        public void Start()
        {
            if (AIName != null)
            {
                WorldState ws = new WorldState();
                entity = GetComponent<CombatEntity>();
                domain = (DomainBase)Activator.CreateInstance(Type.GetType(AIName));
                domain.Init(entity, ws);
                //在其中添加具体的任务
                domain.Init();
                domain.BuildWorldState();
                //初始化预测器
                Planner = new Planner();
                Planner.Init(ws, domain);
                //初始化 执行器
                PlanRunner = new PlanRunner(ws);
            }
        }
        private void Update()
        {
            Planner.BuildPlan();
            PlanRunner.RunPlan(Planner.GetFinalTask());
        }
    }
}