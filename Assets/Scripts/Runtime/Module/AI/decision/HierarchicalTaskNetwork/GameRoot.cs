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
                //��������Ӿ��������
                domain.Init();
                domain.BuildWorldState();
                //��ʼ��Ԥ����
                Planner = new Planner();
                Planner.Init(ws, domain);
                //��ʼ�� ִ����
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